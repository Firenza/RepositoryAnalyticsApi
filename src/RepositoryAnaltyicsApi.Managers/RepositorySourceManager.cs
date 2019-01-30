using Microsoft.Extensions.Caching.Distributed;
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
        private IDistributedCache distributedCache;
        private ILogger<RepositorySourceManager> logger;

        public RepositorySourceManager(ILogger<RepositorySourceManager> logger, IRepositorySourceRepository repositorySourceRepository, IDistributedCache distributedCache)
        {
            this.logger = logger;
            this.repositorySourceRepository = repositorySourceRepository;
            this.distributedCache = distributedCache;
        }

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths, DateTime? asOf = null)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(repositoryOwner, repositoryName, branch, asOf);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            // Assuming we will always be asking for the same files for each repo, should probs get a hash of all the file names as part of cache key
            var multipleFileContentsCacheKey = GetMultipleFileContentsCacheKey(repositoryOwner, repositoryName, branch);

            var filesContentInformation = await distributedCache.GetAsync<List<(string fullFilePath, string fileContent)>>(multipleFileContentsCacheKey);

            if (filesContentInformation == null)
            {
                logger.LogDebug($"retrieving {multipleFileContentsCacheKey} from source");

                filesContentInformation = await repositorySourceRepository.GetMultipleFileContentsAsync(repositoryOwner, repositoryName, gitRef, fullFilePaths).ConfigureAwait(false);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetAsync(multipleFileContentsCacheKey, filesContentInformation, cacheOptions);
            }

            // Go through and cache all the individual files so when we request them individually they can be pulled from the cache
            foreach (var fileContentInformation in filesContentInformation)
            {
                var fileContentCacheKey = GetFileContentCacheKey(repositoryOwner, repositoryName, fileContentInformation.fullFilePath, gitRef);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetStringAsync(fileContentCacheKey, fileContentInformation.fileContent, cacheOptions);
            }

            return filesContentInformation;
        }

        public async Task<string> ReadFileContentAsync(string owner, string name, string branch, string fullFilePath, DateTime? asOf = null)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(owner, name, branch, asOf);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            var cacheKey = GetFileContentCacheKey(owner, name, fullFilePath, gitRef);

            var fileContent = await distributedCache.GetStringAsync(cacheKey);

            if (fileContent == null)
            {
                logger.LogDebug($"Retrieving {cacheKey} from source");

                fileContent = await repositorySourceRepository.ReadFileContentAsync(owner, name, fullFilePath, gitRef);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetStringAsync(cacheKey, fileContent, cacheOptions);
            }

            return fileContent;
        }

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(owner, name, branch, asOf);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            var cacheKey = GetFileListCacheKey(owner, name, gitRef);

            var repositoryFiles = await distributedCache.GetAsync<List<RepositoryFile>>(cacheKey);

            if (repositoryFiles == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                repositoryFiles = await repositorySourceRepository.ReadFilesAsync(owner, name, gitRef);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetAsync(cacheKey, repositoryFiles);
            }

            return repositoryFiles;
        }

        public async Task<CursorPagedResults<RepositorySummary>> ReadRepositorySummariesAsync(string owner, int take, string endCursor)
        {
            var respositorySummaries = new CursorPagedResults<RepositorySummary>();

            var ownerType = await ReadOwnerType(owner);

            if (ownerType == OwnerType.Organization)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(owner, null, take, endCursor);
            }
            else if (ownerType == OwnerType.User)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(null, owner, take, endCursor);
            }

            return respositorySummaries;
        }

        public async Task<RepositorySummary> ReadRepositorySummaryAsync(string owner, string name)
        {
            var repositorySummary = new RepositorySummary();

            var ownerType = await ReadOwnerType(owner);

            var cacheKey = GetRepositorySummaryCacheKey(owner, name);

            if (ownerType == OwnerType.Organization)
            {
                repositorySummary = await distributedCache.GetAsync<RepositorySummary>(cacheKey);

                if (repositorySummary == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(owner, null, name);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(10)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySummary, cacheOptions);
                }
            }
            else if (ownerType == OwnerType.User)
            {
                repositorySummary = await distributedCache.GetAsync<RepositorySummary>(cacheKey);

                if (repositorySummary == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(null, owner, name);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(10)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySummary, cacheOptions);
                }
            }

            return repositorySummary;
        }

        public async Task<RepositorySourceSnapshot> ReadRepositorySourceSnapshotAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var repositorySourceSnapshot = new RepositorySourceSnapshot();

            var ownerType = await ReadOwnerType(owner);

            var cacheKey = GetRepositorySourceSnapshotCacheKey(owner, name, branch, asOf);

            if (ownerType == OwnerType.Organization)
            {
                repositorySourceSnapshot = await distributedCache.GetAsync<RepositorySourceSnapshot>(cacheKey);

                if (repositorySourceSnapshot == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySourceSnapshot = await repositorySourceRepository.ReadRepositorySourceSnapshotAsync(owner, null, name, branch, asOf);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                        SlidingExpiration = TimeSpan.FromSeconds(10)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySourceSnapshot, cacheOptions);
                }
            }
            else if (ownerType == OwnerType.User)
            {
                repositorySourceSnapshot = await distributedCache.GetAsync<RepositorySourceSnapshot>(cacheKey);

                if (repositorySourceSnapshot == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySourceSnapshot = await repositorySourceRepository.ReadRepositorySourceSnapshotAsync(null, owner, name, branch, asOf);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                        SlidingExpiration = TimeSpan.FromSeconds(10)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySourceSnapshot, cacheOptions);
                }
            }

            return repositorySourceSnapshot;
        }

        public async Task<RepositorySourceRepository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var cacheKey = GetRepositroyCacheKey(repositoryOwner, repositoryName);

            var ownerType = await ReadOwnerType(repositoryOwner);

            var repository = await distributedCache.GetAsync<RepositorySourceRepository>(cacheKey);

            if (repository == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                repository = await repositorySourceRepository.ReadRepositoryAsync(repositoryOwner, repositoryName);

                if (ownerType == OwnerType.Organization)
                {
                    var teams = await ReadTeams();
                    repository.Teams = teams;
                }

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetAsync(cacheKey, repository, cacheOptions);
            }

            return repository;

            async Task<List<string>> ReadTeams()
            {
                var orgTeamsCacheKey = GetOrganizationTeamsCacheKey(repositoryOwner);

                var teamToRepositoriesMap = await distributedCache.GetAsync<Dictionary<string, List<string>>>(orgTeamsCacheKey);

                if (teamToRepositoriesMap == null)
                {
                    logger.LogDebug($"retrieving {orgTeamsCacheKey} from source");

                    teamToRepositoriesMap = await this.repositorySourceRepository.ReadTeamToRepositoriesMaps(repositoryOwner);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                        SlidingExpiration = TimeSpan.FromHours(1)
                    };

                    await distributedCache.SetAsync(cacheKey, repository, cacheOptions);
                }

                var teams = teamToRepositoriesMap.Where(kvp => kvp.Value.Contains(repositoryName))?.Select(kvp => kvp.Key);

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

            var ownerType = await distributedCache.GetAsync<OwnerType?>(cacheKey);

            if (ownerType == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                ownerType = await repositorySourceRepository.ReadOwnerType(owner);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    // Set this duration login enough that a scan of all the repositories will only result in one read of the data
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                await distributedCache.SetAsync(cacheKey, ownerType, cacheOptions);
            }

            return ownerType.Value;
        }

        private string GetOrganizationTeamsCacheKey(string organization)
        {
            return $"teams|{organization}";
        }

        private string GetMultipleFileContentsCacheKey(string owner, string name, string gitRef)
        {
            return $"multipleFileContents|{owner}|{name}|{gitRef}";
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

        private string GetRepositorySummaryCacheKey(string owner, string name)
        {
            return $"repositorySummary|{owner}|{name}";
        }

        private string GetRepositorySourceSnapshotCacheKey(string owner, string name, string branch, DateTime? asOf)
        {
            if (asOf.HasValue)
            {
                return $"repositorySourceSnapshot|{owner}|{name}|{branch}|{asOf.Value.Ticks}";
            }
            else
            {
                return $"repositorySourceSnapshot|{owner}|{name}|{branch}";
            }

        }

    }
}
