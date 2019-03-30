using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class RelationalRepositoryCurrentStateRepository : IRepositoryCurrentStateRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private IMapper mapper;

        public RelationalRepositoryCurrentStateRepository(RepositoryAnalysisContext repositoryAnalysisContext, IMapper mapper)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.mapper = mapper;
        }

        public async Task<ServiceModel.RepositoryCurrentState> ReadAsync(string repositoryId)
        {
            ServiceModel.RepositoryCurrentState repositoryCurrentState = null;

            var dbRepositoryCurrentState = await repositoryAnalysisContext
                                            .RepositoryCurrentState
                                            .AsNoTracking()
                                            .Include(rcs => rcs.Teams)
                                            .Include(rcs => rcs.Topics)
                                            .Where(rcs => rcs.RepositoryId == repositoryId)
                                            .SingleOrDefaultAsync();

            if (dbRepositoryCurrentState != null)
            {
                repositoryCurrentState = mapper.Map<ServiceModel.RepositoryCurrentState>(dbRepositoryCurrentState);
            }

            return repositoryCurrentState;
        }

        public async Task<int?> UpsertAsync(ServiceModel.RepositoryCurrentState repositoryCurrentState)
        {
            var dbRepositoryCurrentState = await repositoryAnalysisContext
                                            .RepositoryCurrentState
                                            .Include(rcs => rcs.Teams)
                                            .Include(rcs => rcs.Topics)
                                            .Where(rcs => rcs.RepositoryId == repositoryCurrentState.Id)
                                            .SingleOrDefaultAsync();

            if (dbRepositoryCurrentState == null)
            {
                dbRepositoryCurrentState = mapper.Map<RepositoryCurrentState>(repositoryCurrentState);

                repositoryAnalysisContext.Add(dbRepositoryCurrentState);
            }
            else
            {
                // If the object already exists in the DB, then map the model object into this 
                // already existing db objec to take advantage of EF update tracking
                mapper.Map(repositoryCurrentState, dbRepositoryCurrentState);
            }

            await repositoryAnalysisContext.SaveChangesAsync();

            return dbRepositoryCurrentState.RepositoryCurrentStateId;
        }
    }
}