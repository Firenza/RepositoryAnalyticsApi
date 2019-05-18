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

        public RelationalRepositoryRepository(RepositoryAnalysisContext repositoryAnalysisContext, IMapper mapper)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.mapper = mapper;
        }

        public async Task<Repository> ReadAsync(string repositoryId, DateTime? asOf)
        {
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var respositoryDependencySearchResults = await dbConnection.QueryAsync<Model.EntityFramework.RepositoryCurrentState>(
                @"select RS.*
                    from repository_current_state RS
                    limit 3 
                    offset 2");



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

            var repositorySnapshot = mapper.Map<ServiceModel.RepositorySnapshot>(dbRepositoryCurrentState.RepositorySnapshots.FirstOrDefault());
            var repositoryCurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(dbRepositoryCurrentState);

            return new Repository
            {
                CurrentState = repositoryCurrentState,
                Snapshot = repositorySnapshot
            };
        }

        public async Task<List<ServiceModel.Repository>> ReadMultipleAsync(DateTime? asOf, int? page, int? pageSize)
        {
            var dbRepositoryCurrentState = repositoryAnalysisContext
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
                                 rcs.RepositorySnapshots.Any(rs =>
                                     !asOf.HasValue && rs.WindowEndsOn == null ||
                                     asOf.HasValue && rs.WindowStartsOn < asOf.Value && rs.WindowEndsOn > asOf.Value));


            dbRepositoryCurrentState.Skip(pageSize.Value * page.Value).Take(pageSize ?? 10);

            var repositoryCurrentStates = await dbRepositoryCurrentState.ToListAsync();

            var repositories = mapper.Map<List<ServiceModel.Repository>>(repositoryCurrentStates);

            return repositories;
        }

    }
}
