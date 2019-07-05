using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryTypeManager : IRepositoryTypeManager
    {
        private IRepositoryTypeRepository repositoryTypesRepository;

        public RepositoryTypeManager(IRepositoryTypeRepository repositoryTypesRepository)
        {
            this.repositoryTypesRepository = repositoryTypesRepository;
        }

        public async Task<List<CountAggregationResult>> ReadAllAsync()
        {
            return await this.repositoryTypesRepository.ReadAllAsync();
        }
    }
}
