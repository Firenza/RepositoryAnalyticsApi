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

        public async Task<List<RepositoryDependencySearchResult>> SearchAsync(string name)
        {
            return await dependencyRepository.SearchAsync(name);
        }

        public async Task<List<string>> SearchNamesAsync(string name)
        {
            return await dependencyRepository.SearchNamesAsync(name).ConfigureAwait(false);
        }
    }
}
