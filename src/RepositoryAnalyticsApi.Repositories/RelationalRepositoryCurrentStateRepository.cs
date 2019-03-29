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

        public RelationalRepositoryCurrentStateRepository(RepositoryAnalysisContext repositoryAnalysisContext)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
        }

        public async Task<ServiceModel.RepositoryCurrentState> ReadAsync(string repositoryId)
        {
            var repositoryCurrentSatte = repositoryAnalysisContext
                                            .RepositoryCurrentState
                                            .SingleOrDefault(rcs => rcs.RepositoryId == repositoryId);


            throw new NotImplementedException();
        }

        public async Task<int?> UpsertAsync(ServiceModel.RepositoryCurrentState repositoryCurrentState)
        {

            throw new NotImplementedException();
        }
    }
}