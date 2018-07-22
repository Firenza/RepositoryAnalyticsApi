using RepositoryAnaltyicsApi.Interfaces;
using System;
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

        public async Task<List<string>> ReadAsync(DateTime? asOf)
        {
            return await repositoriesTypeNamesRepository.ReadAsync(asOf);
        }
    }
}