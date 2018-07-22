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

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths, DateTime? asOf = null)
        {
            logger.LogDebug("Retrieving file contents from source");

            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSummary = await ReadRepositorySummaryAsync(repositoryOwner, repositoryName, branch, asOf);
                gitRef = repoSummary.ClosestCommitTreeId;
            }

            var filesContentInformation = await repositorySourceRepository.GetMultipleFileContentsAsync(repositoryOwner, repositoryName, branch, fullFilePaths).ConfigureAwait(false);

            foreach (var fileContentInformation in filesContentInformation)
            {
                var cacheKey = GetFileContentCacheKey(repositoryOwner, repositoryName, fileContentInformation.fullFilePath, gitRef);

                memoryCache.Set<string>(cacheKey, fileContentInformation.fileContent);
            }

            return filesContentInformation;
        }

        public async Task<string> ReadFileContentAsync(string owner, string name, string branch, string fullFilePath, DateTime? asOf = null)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSummary = await ReadRepositorySummaryAsync(owner, name, branch, asOf);
                gitRef = repoSummary.ClosestCommitTreeId;
            }

            var cacheKey = GetFileContentCacheKey(owner, name, fullFilePath, gitRef);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                logger.LogDebug($"Retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                return await repositorySourceRepository.ReadFileContentAsync(owner, name, fullFilePath, gitRef);
            }).ConfigureAwait(false);

            return cacheEntry;
        }

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSummary = await ReadRepositorySummaryAsync(owner, name, branch, asOf);
                gitRef = repoSummary.ClosestCommitTreeId;
            }

            var cacheKey = GetFileListCacheKey(owner, name, gitRef);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
           {
               logger.LogDebug($"retrieving {cacheKey} from source");
               entry.SlidingExpiration = TimeSpan.FromSeconds(10);
               return await repositorySourceRepository.ReadFilesAsync(owner, name, gitRef);
           });

            return cacheEntry;
        }

        public async Task<CursorPagedResults<RepositorySummary>> ReadRepositoriesAsync(string owner, int take, string endCursor, DateTime? asOf)
        {
            var respositorySummaries = new CursorPagedResults<RepositorySummary>();

            var ownerType = await ReadOwnerType(owner);

            if (ownerType == OwnerType.Organization)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(owner, null, take, endCursor, asOf);
            }
            else if (ownerType == OwnerType.User)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(null, owner, take, endCursor, asOf);
            }

            return respositorySummaries;
        }

        public async Task<RepositorySummary> ReadRepositorySummaryAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var respositorySummary = new RepositorySummary();

            var ownerType = await ReadOwnerType(owner);

            var cacheKey = GetRepositorySummaryCacheKey(owner, name, branch, asOf);

            if (ownerType == OwnerType.Organization)
            {
                var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");
                    entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                    respositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(owner, null, name, branch, asOf);

                    return respositorySummary;
                }).ConfigureAwait(false);

                return cacheEntry;
            }
            else if (ownerType == OwnerType.User)
            {
                var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");
                    entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                    respositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(null, owner, name, branch, asOf);

                    return respositorySummary;
                }).ConfigureAwait(false);

                return cacheEntry;
            }

            return respositorySummary;
        }

        public async Task<RepositorySourceRepository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var cacheKey = GetRepositroyCacheKey(repositoryOwner, repositoryName);

            var ownerType = await ReadOwnerType(repositoryOwner);

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                logger.LogDebug($"retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                var repository = await repositorySourceRepository.ReadRepositoryAsync(repositoryOwner, repositoryName);

                if (ownerType == OwnerType.Organization)
                {
                    var teams = await ReadTeams(repositoryOwner);
                    repository.Teams = teams;
                }

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

        public async Task<OwnerType> ReadOwnerType(string owner)
        {
            var cacheKey = $"ownerType|{owner}";

            var cacheEntry = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                logger.LogDebug($"Retrieving {cacheKey} from source");
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return await repositorySourceRepository.ReadOwnerType(owner);
            }).ConfigureAwait(false);

            return cacheEntry;
        }

        private string GetOrganizationTeamsCacheKey(string organization)
        {
            return $"teams|{organization}";
        }

        private string GetFileContentCacheKey(string owner, string name, string fullFilePath, string gitRef)
        {
            return $"fileContent|{owner}|{name}|{fullFilePath}|{gitRef}";
        }

        private string GetFileListCacheKey(string owner, string name, string gitRef)
        {
            return $"fileList|{owner}|{name}|{gitRef}";
        }

        private string GetRepositroyCacheKey(string owner, string name)
        {
            return $"repository|{owner}|{name}";
        }

        private string GetRepositorySummaryCacheKey(string owner, string name, string branch, DateTime? asOf)
        {
            if (asOf.HasValue)
            {
                return $"repositorySummary|{owner}|{name}|{branch}|{asOf.Value.Ticks}";
            }
            else
            {
                return $"repositorySummary|{owner}|{name}|{branch}";
            }
            
        }
    }
}
