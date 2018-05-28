using RepositoryAnaltyicsApi.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoriesTypeNamesManager : IRepositoriesTypeNamesManager
    {
        private IRepositoriesTypeNamesRepository repositoriesTypeNamesRepository;

        public RepositoriesTypeNamesManager(IRepositoriesTypeNamesRepository repositoriesTypeNamesRepository)
        {
            this.repositoriesTypeNamesRepository = repositoriesTypeNamesRepository;
        }

        public async Task<List<string>> ReadAsync()
        {
            return await repositoriesTypeNamesRepository.ReadAsync();
        }
    }
}