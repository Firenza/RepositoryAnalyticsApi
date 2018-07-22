using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class DependencyManager : IDependencyManager
    {
        private IDependencyRepository dependencyRepository;

        public DependencyManager(IDependencyRepository dependencyRepository)
        {
            this.dependencyRepository = dependencyRepository;
        }

        public async Task<List<RepositoryDependencySearchResult>> SearchAsync(string name, DateTime? asOf)
        {
            return await dependencyRepository.SearchAsync(name, asOf).ConfigureAwait(false);
        }

        public async Task<List<string>> SearchNamesAsync(string name, DateTime? asOf)
        {
            return await dependencyRepository.SearchNamesAsync(name, asOf).ConfigureAwait(false);
        }
    }
}
