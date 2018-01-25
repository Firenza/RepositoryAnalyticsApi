using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositoryManager
    {
        private IRepositoryRepository repositoryRepository;

        public RepositoryManager(IRepositoryRepository repositoryRepository)
        {
            this.repositoryRepository = repositoryRepository;
        }

        public void Create(Repository repository)
        {
            repositoryRepository.Create(repository);
        }

        public void Delete(string id)
        {
            repositoryRepository.Delete(id);
        }

        public Repository Read(string id)
        {
            return repositoryRepository.Read(id);
        }

        public void Update(Repository repository)
        {
            repositoryRepository.Update(repository);
        }
    }
}
