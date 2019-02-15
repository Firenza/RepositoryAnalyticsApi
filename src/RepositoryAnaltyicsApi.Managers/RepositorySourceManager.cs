using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.InternalModel;
using RepositoryAnalyticsApi.InternalModel.AppSettings;
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
        private Caching cachingSettings;

        public RepositorySourceManager(
            ILogger<RepositorySourceManager> logger, 
            IRepositorySourceRepository repositorySourceRepository, 
            IDistributedCache distributedCache, 
            Caching cachingSettings
        )
        {
            this.logger = logger;
            this.repositorySourceRepository = repositorySourceRepository;
            this.distributedCache = distributedCache;
            this.cachingSettings = cachingSettings;
        }

        public async Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths, DateTime? asOf = null)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(repositoryOwner, repositoryName, branch, asOf).ConfigureAwait(false);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            // Assuming we will always be asking for the same files for each repo, should probs get a hash of all the file names as part of cache key
            var multipleFileContentsCacheKey = GetMultipleFileContentsCacheKey(repositoryOwner, repositoryName, branch);

            var filesContentInformation = await distributedCache.GetAsync<List<(string fullFilePath, string fileContent)>>(multipleFileContentsCacheKey).ConfigureAwait(false);

            if (filesContentInformation == null)
            {
                logger.LogDebug($"retrieving {multipleFileContentsCacheKey} from source");

                filesContentInformation = await repositorySourceRepository.GetMultipleFileContentsAsync(repositoryOwner, repositoryName, gitRef, fullFilePaths).ConfigureAwait(false);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                };

                await distributedCache.SetAsync(multipleFileContentsCacheKey, filesContentInformation, cacheOptions).ConfigureAwait(false);
            }

            // Go through and cache all the individual files so when we request them individually they can be pulled from the cache
            foreach (var fileContentInformation in filesContentInformation)
            {
                var fileContentCacheKey = GetFileContentCacheKey(repositoryOwner, repositoryName, fileContentInformation.fullFilePath, gitRef);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                };

                // For whatever reason the current call to get repo file content sometimes returns null even when the file has content
                // so just cache an empty string as you can't cache a null value
                // TODO: Figrure out exactly what's going on here
                await distributedCache.SetStringAsync(fileContentCacheKey, fileContentInformation.fileContent ?? string.Empty, cacheOptions).ConfigureAwait(false);
            }

            return filesContentInformation;
        }

        public async Task<string> ReadFileContentAsync(string owner, string name, string branch, string fullFilePath, DateTime? asOf = null)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(owner, name, branch, asOf).ConfigureAwait(false);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            var cacheKey = GetFileContentCacheKey(owner, name, fullFilePath, gitRef);

            var fileContent = await distributedCache.GetStringAsync(cacheKey).ConfigureAwait(false);

            if (fileContent == null)
            {
                logger.LogDebug($"Retrieving {cacheKey} from source");

                fileContent = await repositorySourceRepository.ReadFileContentAsync(owner, name, fullFilePath, gitRef);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                };

                await distributedCache.SetStringAsync(cacheKey, fileContent, cacheOptions).ConfigureAwait(false);
            }

            return fileContent;
        }

        public async Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var gitRef = branch;

            if (asOf.HasValue)
            {
                var repoSourceSnapshot = await ReadRepositorySourceSnapshotAsync(owner, name, branch, asOf).ConfigureAwait(false);
                gitRef = repoSourceSnapshot.ClosestCommitTreeId;
            }

            var cacheKey = GetFileListCacheKey(owner, name, gitRef);

            var repositoryFiles = await distributedCache.GetAsync<List<RepositoryFile>>(cacheKey).ConfigureAwait(false);

            if (repositoryFiles == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                repositoryFiles = await repositorySourceRepository.ReadFilesAsync(owner, name, gitRef).ConfigureAwait(false);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                };

                await distributedCache.SetAsync(cacheKey, repositoryFiles).ConfigureAwait(false);
            }

            return repositoryFiles;
        }

        public async Task<CursorPagedResults<RepositorySummary>> ReadRepositorySummariesAsync(string owner, int take, string endCursor)
        {
            var respositorySummaries = new CursorPagedResults<RepositorySummary>();

            var ownerType = await ReadOwnerType(owner).ConfigureAwait(false);

            if (ownerType == OwnerType.Organization)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(owner, null, take, endCursor).ConfigureAwait(false);
            }
            else if (ownerType == OwnerType.User)
            {
                respositorySummaries = await repositorySourceRepository.ReadRepositorySummariesAsync(null, owner, take, endCursor).ConfigureAwait(false);
            }

            return respositorySummaries;
        }

        public async Task<RepositorySummary> ReadRepositorySummaryAsync(string owner, string name)
        {
            var repositorySummary = new RepositorySummary();

            var ownerType = await ReadOwnerType(owner).ConfigureAwait(false);

            var cacheKey = GetRepositorySummaryCacheKey(owner, name);

            if (ownerType == OwnerType.Organization)
            {
                repositorySummary = await distributedCache.GetAsync<RepositorySummary>(cacheKey).ConfigureAwait(false);

                if (repositorySummary == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(owner, null, name).ConfigureAwait(false);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySummary, cacheOptions).ConfigureAwait(false);
                }
            }
            else if (ownerType == OwnerType.User)
            {
                repositorySummary = await distributedCache.GetAsync<RepositorySummary>(cacheKey).ConfigureAwait(false);

                if (repositorySummary == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySummary = await repositorySourceRepository.ReadRepositorySummaryAsync(null, owner, name).ConfigureAwait(false);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySummary, cacheOptions).ConfigureAwait(false);
                }
            }

            return repositorySummary;
        }

        public async Task<RepositorySourceSnapshot> ReadRepositorySourceSnapshotAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var repositorySourceSnapshot = new RepositorySourceSnapshot();

            var ownerType = await ReadOwnerType(owner).ConfigureAwait(false);

            var cacheKey = GetRepositorySourceSnapshotCacheKey(owner, name, branch, asOf);

            if (ownerType == OwnerType.Organization)
            {
                repositorySourceSnapshot = await distributedCache.GetAsync<RepositorySourceSnapshot>(cacheKey).ConfigureAwait(false);

                if (repositorySourceSnapshot == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySourceSnapshot = await repositorySourceRepository.ReadRepositorySourceSnapshotAsync(owner, null, name, branch, asOf).ConfigureAwait(false);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySourceSnapshot, cacheOptions).ConfigureAwait(false);
                }
            }
            else if (ownerType == OwnerType.User)
            {
                repositorySourceSnapshot = await distributedCache.GetAsync<RepositorySourceSnapshot>(cacheKey).ConfigureAwait(false);

                if (repositorySourceSnapshot == null)
                {
                    logger.LogDebug($"retrieving {cacheKey} from source");

                    repositorySourceSnapshot = await repositorySourceRepository.ReadRepositorySourceSnapshotAsync(null, owner, name, branch, asOf).ConfigureAwait(false);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                    };

                    await distributedCache.SetAsync(cacheKey, repositorySourceSnapshot, cacheOptions).ConfigureAwait(false);
                }
            }

            return repositorySourceSnapshot;
        }

        public async Task<RepositorySourceRepository> ReadRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var cacheKey = GetRepositroyCacheKey(repositoryOwner, repositoryName);

            var ownerType = await ReadOwnerType(repositoryOwner).ConfigureAwait(false);

            var repository = await distributedCache.GetAsync<RepositorySourceRepository>(cacheKey).ConfigureAwait(false);

            if (repository == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                repository = await repositorySourceRepository.ReadRepositoryAsync(repositoryOwner, repositoryName).ConfigureAwait(false);

                if (ownerType == OwnerType.Organization)
                {
                    var teams = await ReadTeams().ConfigureAwait(false);
                    repository.Teams = teams;
                }

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.RepositoryData)
                };

                await distributedCache.SetAsync(cacheKey, repository, cacheOptions).ConfigureAwait(false);
            }


            return repository;

            async Task<List<RepositoryTeam>> ReadTeams()
            {
                var orgTeamsCacheKey = GetOrganizationTeamsCacheKey(repositoryOwner);

                var teamToRepositoriesMap = await distributedCache.GetAsync<Dictionary<string, List<TeamRepositoryConnection>>>(orgTeamsCacheKey).ConfigureAwait(false);

                if (teamToRepositoriesMap == null)
                {
                    logger.LogDebug($"retrieving {orgTeamsCacheKey} from source");

                    teamToRepositoriesMap = await this.repositorySourceRepository.ReadTeamToRepositoriesMaps(repositoryOwner).ConfigureAwait(false);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.OrganizationTeams)
                    };

                    await distributedCache.SetAsync(orgTeamsCacheKey, teamToRepositoriesMap, cacheOptions).ConfigureAwait(false);
                }

                var repositoryTeams = new List<RepositoryTeam>(); 

                foreach (var kvp in teamToRepositoriesMap)
                {
                    var matchingTeamRepositoryConnection = kvp.Value.FirstOrDefault(trc => string.Equals(trc.RepositoryName, repositoryName));

                    if (matchingTeamRepositoryConnection != null)
                    {
                        var repositoryTeam = new RepositoryTeam
                        {
                            Name = kvp.Key,
                            Permission = matchingTeamRepositoryConnection.TeamPermissions
                        };

                        repositoryTeams.Add(repositoryTeam);
                    }
                }

                return repositoryTeams;
            }
        }

        public async Task<OwnerType> ReadOwnerType(string owner)
        {
            var cacheKey = $"ownerType|{owner}";

            var ownerType = await distributedCache.GetAsync<OwnerType?>(cacheKey).ConfigureAwait(false);

            if (ownerType == null)
            {
                logger.LogDebug($"retrieving {cacheKey} from source");

                ownerType = await repositorySourceRepository.ReadOwnerType(owner).ConfigureAwait(false);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(cachingSettings.Durations.OwnerType)
                };

                await distributedCache.SetAsync(cacheKey, ownerType, cacheOptions).ConfigureAwait(false);
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
