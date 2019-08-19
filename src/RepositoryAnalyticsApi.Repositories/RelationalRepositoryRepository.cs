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
              {{DEPENDENCY_JOIN}}
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

            query = query.Replace("{{DEPENDENCY_JOIN}}", BuildDependenciesJoin());

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

            string BuildDependenciesJoin()
            {
                if (repositorySearch.Dependencies.Any())
                {
                    var depdendencyJoinStringBuilder = new StringBuilder();
                    depdendencyJoinStringBuilder.AppendLine("and rd.repository_snapshot_id in (");

                    var dependenciesAdded = 0;

                    foreach (var dependency in repositorySearch.Dependencies)
                    {
                        if (dependenciesAdded > 0)
                        {
                            depdendencyJoinStringBuilder.AppendLine("intersect");
                        }

                        depdendencyJoinStringBuilder.AppendLine("select repository_snapshot_id");
                        depdendencyJoinStringBuilder.AppendLine("from public.repository_dependency");
                        depdendencyJoinStringBuilder.AppendLine($"where name ilike '{dependency.Name}'");

                        if (!string.IsNullOrWhiteSpace(dependency.Version))
                        {
                            var paddedVersion = versionManager.GetPaddedVersion(dependency.Version);

                            var rangeSpecifierText = GetRangeSpecifierString(dependency.RangeSpecifier);

                            depdendencyJoinStringBuilder.AppendLine($"and padded_version {rangeSpecifierText} '{paddedVersion}'");
                        }

                        dependenciesAdded++;
                    }

                    depdendencyJoinStringBuilder.AppendLine(")");

                    return depdendencyJoinStringBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
