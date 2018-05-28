using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositorySnapshotManager
    {
        private IRepositorySnapshotRepository repositoryRepository;

        public RepositoryManager(IRepositorySnapshotRepository repositoryRepository)
        {
            this.repositoryRepository = repositoryRepository;
        }

        public async Task CreateAsync(RepositorySnapshot repository)
        {
            await repositoryRepository.CreateAsync(repository).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string id)
        {
            await repositoryRepository.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<RepositorySnapshot> ReadAsync(string id)
        {
            return await repositoryRepository.ReadAsync(id).ConfigureAwait(false);
        }

        public async Task<List<RepositorySnapshot>> SearchAsync(RepositorySearch repositorySearch)
        {
            return await repositoryRepository.SearchAsync(repositorySearch);
        }

        public async Task UpdateAsync(RepositorySnapshot repository)
        {
            await repositoryRepository.UpdateAsync(repository).ConfigureAwait(false);
        }
    }
}
