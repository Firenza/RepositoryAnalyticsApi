using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositorySourceManager : IRepositorySourceManager
    {
        private IRepositorySourceRepository repositorySourceRepository;
        private IMemoryCache memoryCache;
        private ILogger<RepositorySourceManager> logger;

        public RepositorySourceManager(ILogger<RepositorySourceManager> logger, IRepositorySourceRepository repositorySourceRepository, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.repositorySourceRepository = repositorySourceRepository;
            this.memoryCache = memoryCache;
        }

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths)
        {
            logger.LogDebug("Retrieving file contents from source");

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
                logger.LogDebug($"Retrieving {cacheKey} from source");
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
                logger.LogDebug($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                return await repositorySourceRepository.ReadFilesAsync(owner, name, branch);
            });

            return cacheEntry;
        }

        public async Task<CursorPagedResults<RepositorySummary>> ReadRepositoriesAsync(string organization, string user, int take, string endCursor, DateTime? asOf)
        {
            var repositories = await repositorySourceRepository.ReadRepositorySummariesAsync(organization, user, take, endCursor, asOf);

            return repositories;
        }

        public async Task<Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var cacheKey = GetRepositroyCacheKey(repositoryOwner, repositoryName);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                logger.LogDebug($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                var repository = await repositorySourceRepository.ReadRepositoryAsync(repositoryOwner, repositoryName);
                var teams = await ReadTeams(repositoryOwner);
                repository.Teams = teams;
                return repository;
            }).ConfigureAwait(false);

            return cacheEntry;

            async Task<List<string>> ReadTeams(string repository)
            {
                var teamsCacheKey = GetOrganizationTeamsCacheKey(repositoryOwner);

                var teamCacheEntry = await memoryCache.GetOrCreateAsync(teamsCacheKey, async entry =>
                {
                    logger.LogDebug($"retrieving {teamsCacheKey} from source");
                    // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                    entry.SlidingExpiration = TimeSpan.FromHours(1);
                    var teamToRepsoitoriesMap = await this.repositorySourceRepository.ReadTeamToRepositoriesMaps(repositoryOwner);
                    return teamToRepsoitoriesMap;
                }).ConfigureAwait(false);


                var teams = teamCacheEntry.Where(kvp => kvp.Value.Contains(repositoryName))?.Select(kvp => kvp.Key);

                if (teams != null && teams.Any())
                {
                    return teams.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        private string GetOrganizationTeamsCacheKey(string organization)
        {
            return $"teams|{organization}";
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
