using Microsoft.Extensions.Caching.Distributed;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.InternalModel.AppSettings;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class DependencyManager : IDependencyManager
    {
        private IDependencyRepository dependencyRepository;
        private IDistributedCache distributedCache;
        private Caching cachingSettings;

        public DependencyManager(IDependencyRepository dependencyRepository, IDistributedCache distributedCache, Caching cachingSettings)
        {
            this.dependencyRepository = dependencyRepository;
            this.distributedCache = distributedCache;
            this.cachingSettings = cachingSettings;
        }

        public async Task<List<RepositoryDependencySearchResult>> ReadAsync(string name, RepositorySearch repositorySearch)
        {
            return await dependencyRepository.ReadAsync(name, repositorySearch).ConfigureAwait(false);
        }

        public async Task<List<string>> SearchNamesAsync(string name, DateTime? asOf)
        {
            var dependencyNamesCacheKey = name.ToLower();
            
            var dependencyNames = await distributedCache.GetAsync<List<string>>(dependencyNamesCacheKey).ConfigureAwait(false);

            if (dependencyNames == null)
            {
                dependencyNames = await dependencyRepository.SearchNamesAsync(name, asOf).ConfigureAwait(false);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.DependencyNameSearchResults)
                };

                await distributedCache.SetAsync(dependencyNamesCacheKey, dependencyNames, cacheOptions).ConfigureAwait(false);
            }

            return dependencyNames;
        }
    }
}
