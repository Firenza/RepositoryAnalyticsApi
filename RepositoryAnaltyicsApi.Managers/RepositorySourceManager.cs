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

        public List<(string fullFilePath, string fileContent)> GetMultipleFileContents(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths)
        {
            Console.WriteLine($"retrieving file contents from source");

            var filesContentInformation = repositorySourceRepository.GetMultipleFileContents(repositoryOwner, repositoryName, branch, fullFilePaths);

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

        public List<RepositoryFile> ReadFiles(string owner, string name, string branch)
        {
            var cacheKey = GetFileListCacheKey(owner, name, branch);

            var cacheEntry = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                Console.WriteLine($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                return repositorySourceRepository.ReadFiles(owner, name, branch);
            });

            return cacheEntry;
        }

        public List<Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage)
        {
            Console.WriteLine($"retrieving repositories from source");

            return repositorySourceRepository.ReadRepositories(group, pageCount, pageSize, startPage);
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
