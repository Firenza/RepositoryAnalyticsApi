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

        public async Task<List<RepositoryDependency>> SearchAsync(string name)
        {
            return await dependencyRepository.SearchAsync(name).ConfigureAwait(false);
        }
    }
}
