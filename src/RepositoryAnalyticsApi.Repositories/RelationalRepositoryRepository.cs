using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class RelationalRepositoryRepository : IRepositoryRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private IMapper mapper;
        private IVersionManager versionManager;

        public RelationalRepositoryRepository(RepositoryAnalysisContext repositoryAnalysisContext, IMapper mapper, IVersionManager versionManager)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.mapper = mapper;
            this.versionManager = versionManager;
        }

        public async Task<Repository> ReadAsync(string repositoryId, DateTime? asOf)
        {

            var dbRepositoryCurrentState = await repositoryAnalysisContext
                                .RepositoryCurrentState
                                .AsNoTracking()
                                .Include(rcs => rcs.RepositorySnapshots)
                                  .ThenInclude(rs => rs.Dependencies)
                                .Include(rcs => rcs.RepositorySnapshots)
                                  .ThenInclude(rs => rs.Files)
                                .Include(rcs => rcs.RepositorySnapshots)
                                  .ThenInclude(rs => rs.TypesAndImplementations)
                                      .ThenInclude(rti => rti.Implementations)
                                .Where(rcs =>
                                     rcs.RepositoryId == repositoryId &&
                                     rcs.RepositorySnapshots.Any(rs =>
                                         !asOf.HasValue && rs.WindowEndsOn == null ||
                                         asOf.HasValue && rs.WindowStartsOn < asOf.Value && rs.WindowEndsOn > asOf.Value))
                                .SingleOrDefaultAsync();

            if (dbRepositoryCurrentState != null)
            {
                var repository = new Repository
                {
                    CurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(dbRepositoryCurrentState),
                    Snapshot = mapper.Map<ServiceModel.RepositorySnapshot>(dbRepositoryCurrentState.RepositorySnapshots.FirstOrDefault())
                };

                return repository;
            }
            else
            {
                return null;
            }

            //var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            //var dbrcss = new List<Model.EntityFramework.RepositoryCurrentState>();

            //var respositoryDependencySearchResults = await dbConnection.QueryAsync<Model.EntityFramework.RepositoryCurrentState, Model.EntityFramework.RepositorySnapshot, Model.EntityFramework.RepositoryDependency, ServiceModel.Repository>(
            //    @"select * 
            //        from repository_current_state RCS
            //        join repository_snapshot RS
	           //         on RS.repository_current_state_id = RCS.repository_current_state_id
            //        join repository_dependency RD
	           //         on RD.repository_snapshot_id = RS.repository_snapshot_id
            //        where RCS.repository_id = @RepositoryId",

            //(rcs, rs, rd) =>
            //{
            //    var dbrcs = dbrcss.FirstOrDefault(dbrcsx => dbrcsx.RepositoryCurrentStateId == rcs.RepositoryCurrentStateId);

            //    // First add the rcs
            //    if (dbrcs == null)
            //    {
            //        dbrcss.Add(rcs);
            //        dbrcs = rcs;
            //    }

            //    if(dbrcs.RepositorySnapshots == null)
            //    {
            //        dbrcs.RepositorySnapshots = new List<Model.EntityFramework.RepositorySnapshot>();
            //    }

            //    var rsnappp = dbrcs.RepositorySnapshots?.FirstOrDefault(rsnap => rsnap.RepositorySnapshotId == rsnap.RepositorySnapshotId);

            //    if (rsnappp == null)
            //    {
            //        dbrcs.RepositorySnapshots.Add(rs);
            //        rsnappp = rs;
            //    }

            //    if (rsnappp.Dependencies == null)
            //    {
            //        rsnappp.Dependencies = new List<Model.EntityFramework.RepositoryDependency>();
            //    }

            //    var dep = rsnappp.Dependencies.FirstOrDefault(xyz => xyz.RepositoryDependencyId == rd.RepositoryDependencyId);

            //    if (dep == null)
            //    {
            //        rsnappp.Dependencies.Add(rd);
            //        dep = rd;
            //    }

            //    return null;
            //},
            //new { RepositoryId = repositoryId },
            //splitOn: "repository_snapshot_id, repository_dependency_id");

            //var repositories = new List<ServiceModel.Repository>();

            //foreach (var x in dbrcss)
            //{
            //    var repositorySnapshot = mapper.Map<ServiceModel.RepositorySnapshot>(x.RepositorySnapshots.FirstOrDefault());
            //    var repositoryCurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(x);

            //    var newR =  new Repository
            //    {
            //        CurrentState = repositoryCurrentState,
            //        Snapshot = repositorySnapshot
            //    };

            //    repositories.Add(newR);
            //}


            //return repositories.FirstOrDefault();
        }

        public async Task<List<ServiceModel.Repository>> ReadMultipleAsync(DateTime? asOf, int? page, int? pageSize)
        {
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            // First get all the Id's of the repoistory records in this "page"
            var repositoryIdsInPage = await dbConnection.QueryAsync<string>(
              @" select repository_id
                    from repository_current_state
                    limit @limit
                    offset @offset",
                new { limit = pageSize.Value, offset = page.Value * pageSize.Value });

            var dbRepositoryCurrentStates = await repositoryAnalysisContext
                      .RepositoryCurrentState
                      .AsNoTracking()
                      .Include(rcs => rcs.RepositorySnapshots)
                        .ThenInclude(rs => rs.Dependencies)
                      .Include(rcs => rcs.RepositorySnapshots)
                        .ThenInclude(rs => rs.Files)
                      .Include(rcs => rcs.RepositorySnapshots)
                        .ThenInclude(rs => rs.TypesAndImplementations)
                            .ThenInclude(rti => rti.Implementations)
                      .Where(rcs =>
                           repositoryIdsInPage.Contains(rcs.RepositoryId) &&
                           (!rcs.RepositorySnapshots.Any() ||
                           rcs.RepositorySnapshots.Any(rs =>
                               !asOf.HasValue && rs.WindowEndsOn == null ||
                               asOf.HasValue && rs.WindowStartsOn < asOf.Value && rs.WindowEndsOn > asOf.Value)))
                      .ToListAsync();

            var repositories = new List<Repository>();

            foreach (var dbRepositoryCurrentState in dbRepositoryCurrentStates)
            {
                var repository = new Repository
                {
                    CurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(dbRepositoryCurrentState),
                    Snapshot = mapper.Map<ServiceModel.RepositorySnapshot>(dbRepositoryCurrentState.RepositorySnapshots.FirstOrDefault())
                };

                repositories.Add(repository);
            }

            return repositories;

          //  var dbrcss = new List<Model.EntityFramework.RepositoryCurrentState>();

          //  var respositoryDependencySearchResults = await dbConnection.QueryAsync<Model.EntityFramework.RepositoryCurrentState, Model.EntityFramework.RepositorySnapshot, Model.EntityFramework.RepositoryDependency, ServiceModel.Repository>(
          //      @"select * 
          //          from repository_current_state RCS
          //          join repository_snapshot RS
	         //           on RS.repository_current_state_id = RCS.repository_current_state_id
          //          join repository_dependency RD
	         //           on RD.repository_snapshot_id = RS.repository_snapshot_id
          //          where RCS.repository_current_state_id in (
          //              select repository_current_state_id
          //              from repository_current_state
          //              limit @limit
          //              offset @offset
          //          )",

          //  (rcs, rs, rd) =>
          //  {
          //      var dbrcs = dbrcss.FirstOrDefault(dbrcsx => dbrcsx.RepositoryCurrentStateId == rcs.RepositoryCurrentStateId);

          //      // First add the rcs
          //      if (dbrcs == null)
          //      {
          //          dbrcss.Add(rcs);
          //          dbrcs = rcs;
          //      }

          //      if (dbrcs.RepositorySnapshots == null)
          //      {
          //          dbrcs.RepositorySnapshots = new List<Model.EntityFramework.RepositorySnapshot>();
          //      }

          //      var rsnappp = dbrcs.RepositorySnapshots?.FirstOrDefault(rsnap => rsnap.RepositorySnapshotId == rsnap.RepositorySnapshotId);

          //      if (rsnappp == null)
          //      {
          //          dbrcs.RepositorySnapshots.Add(rs);
          //          rsnappp = rs;
          //      }

          //      if (rsnappp.Dependencies == null)
          //      {
          //          rsnappp.Dependencies = new List<Model.EntityFramework.RepositoryDependency>();
          //      }

          //      var dep = rsnappp.Dependencies.FirstOrDefault(xyz => xyz.RepositoryDependencyId == rd.RepositoryDependencyId);

          //      if (dep == null)
          //      {
          //          rsnappp.Dependencies.Add(rd);
          //          dep = rd;
          //      }

          //      return null;
          //  },
          //  new { limit = pageSize.Value, offset = page.Value * pageSize.Value},
          //  splitOn: "repository_snapshot_id, repository_dependency_id");

          //  var repositories = new List<ServiceModel.Repository>();

          //  foreach (var x in dbrcss)
          //  {
          //      var repositorySnapshot = mapper.Map<ServiceModel.RepositorySnapshot>(x.RepositorySnapshots.FirstOrDefault());
          //      var repositoryCurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(x);

          //      var newR = new Repository
          //      {
          //          CurrentState = repositoryCurrentState,
          //          Snapshot = repositorySnapshot
          //      };

          //      repositories.Add(newR);
          //  }


          //  return repositories;
        }

        public async Task<List<string>> SearchAsync(RepositorySearch repositorySearch)
        {
            var query = @"
            select rcs.name
            from repository_current_state as rcs
            join repository_team as rt
              on rt.repository_current_state_id = rcs.repository_current_state_id
            join repository_snapshot as rs
              on rs.repository_current_state_id = rcs.repository_current_state_id
            join repository_type_and_implementations as rti 
              on rti.repository_snapshot_id = rs.repository_snapshot_id
            join repository_implementation as ri 
              on ri.repository_type_and_implementations_id = rti.repository_type_and_implementations_id
            join repository_dependency as rd
              on rd.repository_snapshot_id = rs.repository_snapshot_id
            where 1=1
            {{WHERE_CLAUSES}}
            group by rcs.name
            order by rcs.name
            ";

            var whereClausesStringBuilder = new StringBuilder();

            if (!repositorySearch.AsOf.HasValue)
            {
                whereClausesStringBuilder.AppendLine("and rs.window_ends_on is null");
            }

            if (repositorySearch.HasContinuousDelivery.HasValue)
            {
                whereClausesStringBuilder.AppendLine($"and rcs.continuous_delivery = {repositorySearch.HasContinuousDelivery.ToString()}");
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.Team))
            {
                whereClausesStringBuilder.AppendLine($"and rt.name = '{repositorySearch.Team}'");
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.TeamPermissions))
            {
                whereClausesStringBuilder.AppendLine($"and rt.permission = '{repositorySearch.TeamPermissions}'");
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.TypeName))
            {
                whereClausesStringBuilder.AppendLine($"and rti.type_name = '{repositorySearch.TypeName}'");
            }

            // For now just support one dependency filter
            if (repositorySearch.Dependencies.Any())
            {
                whereClausesStringBuilder.AppendLine($"and rd.name = '{repositorySearch.Dependencies.First().Name}'");

                if (!string.IsNullOrWhiteSpace(repositorySearch.Dependencies.First().Version))
                {
                    var paddedVersion = versionManager.GetPaddedVersion(repositorySearch.Dependencies.First().Version);

                    var rangeSpecifierText = GetRangeSpecifierString(repositorySearch.Dependencies.First().RangeSpecifier);

                    whereClausesStringBuilder.AppendLine($"and rd.padded_version {rangeSpecifierText} '{paddedVersion}'");
                }
            }

            query = query.Replace("{{WHERE_CLAUSES}}", whereClausesStringBuilder.ToString());
            
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var repositoryNames = await dbConnection.QueryAsync<string>(query);

            return repositoryNames.AsList();

            string GetRangeSpecifierString(RangeSpecifier rangeSpecifier)
            {
                switch (rangeSpecifier)
                {
                    case RangeSpecifier.GreaterThan:
                        return ">";
                    case RangeSpecifier.GreaterThanOrEqualTo:
                        return ">=";
                    case RangeSpecifier.LessThan:
                        return "<";
                    case RangeSpecifier.LessThanOrEqualTo:
                        return "<=";
                    case RangeSpecifier.EqualTo:
                        return "=";
                    default:
                        return null;
                }
            }
        }
    }
}
