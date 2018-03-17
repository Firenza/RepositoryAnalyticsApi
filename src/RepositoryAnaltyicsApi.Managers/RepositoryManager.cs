using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositoriesManager
    {
        private IRepositoriesRepository repositoryRepository;

        public RepositoryManager(IRepositoriesRepository repositoryRepository)
        {
            this.repositoryRepository = repositoryRepository;
        }

        public async Task CreateAsync(Repository repository)
        {
            await repositoryRepository.CreateAsync(repository).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string id)
        {
            await repositoryRepository.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<Repository> ReadAsync(string id)
        {
            return await repositoryRepository.ReadAsync(id).ConfigureAwait(false);
        }

        public async Task<List<Repository>> SearchAsync(string typeName, string implementationName, List<(string Name, string Version)> dependencies)
        {
            return await repositoryRepository.SearchAsync(typeName, implementationName, dependencies);
        }

        public async Task UpdateAsync(Repository repository)
        {
            await repositoryRepository.UpdateAsync(repository).ConfigureAwait(false);
        }
    }
}
