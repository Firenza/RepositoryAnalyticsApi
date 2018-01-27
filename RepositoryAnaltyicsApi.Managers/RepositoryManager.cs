using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositoryManager
    {
        private IRepositoryRepository repositoryRepository;

        public RepositoryManager(IRepositoryRepository repositoryRepository)
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

        public async Task UpdateAsync(Repository repository)
        {
            await repositoryRepository.UpdateAsync(repository).ConfigureAwait(false);
        }
    }
}
