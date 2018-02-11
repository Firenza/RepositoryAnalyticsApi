using Microsoft.Extensions.Caching.Memory;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositorySourceManager : IRepositorySourceManager
    {
        private IRepositorySourceRepository repositorySourceRepository;
        private IMemoryCache memoryCache;

        public RepositorySourceManager(IRepositorySourceRepository repositorySourceRepository, IMemoryCache memoryCache)
        {
            this.repositorySourceRepository = repositorySourceRepository;
            this.memoryCache = memoryCache;
        }

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths)
        {
            Console.WriteLine($"retrieving file contents from source");

            var filesContentInformation = await repositorySourceRepository.GetMultipleFileContentsAsync(repositoryOwner, repositoryName, branch, fullFilePaths).ConfigureAwait(false);

            foreach (var fileContentInformation in filesContentInformation)
            {
                var cacheKey = GetFileContentCacheKey(repositoryOwner, repositoryName, fileContentInformation.fullFilePath);

                memoryCache.Set<string>(cacheKey, fileContentInformation.fileContent);
            }

            return filesContentInformation;
        }

        public async Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath)
        {
            var cacheKey = GetFileContentCacheKey(owner, name, fullFilePath);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                Console.WriteLine($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                return await repositorySourceRepository.ReadFileContentAsync(owner, name, fullFilePath);
            }).ConfigureAwait(false);

            return cacheEntry;
        }

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch)
        {
            var cacheKey = GetFileListCacheKey(owner, name, branch);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey,  async entry =>
            {
                Console.WriteLine($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                return await repositorySourceRepository.ReadFilesAsync(owner, name, branch);
            });

            return cacheEntry;
        }

        public async Task<Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var cacheKey = GetRepositroyCacheKey(repositoryOwner, repositoryName);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                Console.WriteLine($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                var repository = await this.repositorySourceRepository.ReadRepositoryAsync(repositoryOwner, repositoryName);
                return repository;
            }).ConfigureAwait(false);

            return cacheEntry;
        }

        private string GetFileContentCacheKey(string owner, string name, string fullFilePath)
        {
            return $"fileContent|{owner}|{name}|{fullFilePath}";
        }

        private string GetFileListCacheKey(string owner, string name, string fullFilePath)
        {
            return $"fileList|{owner}|{name}|{fullFilePath}";
        }

        private string GetRepositroyCacheKey(string owner, string name)
        {
            return $"repository|{owner}|{name}";
        }
    }
}
