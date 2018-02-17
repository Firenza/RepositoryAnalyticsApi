using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryAnalysisManager : IRepositoryAnalysisManager
    {
        private IRepositoryManager repositoryManager;
        private IRepositorySourceManager repositorySourceManager;
        private IEnumerable<IDependencyScraperManager> dependencyScraperManagers;

        public RepositoryAnalysisManager(IRepositoryManager repositoryManager, IRepositorySourceManager repositorySourceManager, IEnumerable<IDependencyScraperManager> dependencyScraperManagers)
        {
            this.repositoryManager = repositoryManager;
            this.repositorySourceManager = repositorySourceManager;
            this.dependencyScraperManagers = dependencyScraperManagers;
        }

        public async Task CreateAsync(RepositoryAnalysis repositoryAnalysis)
        {
            var repository = await repositoryManager.ReadAsync(repositoryAnalysis.RepositoryUrl);
            Repository repositorySourceRepository = null;

            var now = DateTime.Now;

            if (repository == null)
            {
                repository = new Repository();
                repository.CreatedOn = now;
            }

            var parsedRepoUrl = ParseRepositoryUrl();

            var repositoryNeedsUpdating = false;

            if (repositoryAnalysis.ForceCompleteRefresh)
            {
                repositoryNeedsUpdating = true;
            }
            else if(repositoryAnalysis.LastUpdatedOn.HasValue)
            {
                repositoryNeedsUpdating = repository.LastUpdatedOn < repositoryAnalysis.LastUpdatedOn.Value;
            }
            else
            {
                repositorySourceRepository = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);

                repositoryNeedsUpdating = repository.LastUpdatedOn < repositorySourceRepository.LastUpdatedOn;
            }
  
            if (repositoryNeedsUpdating)
            {
                if (repositorySourceRepository == null)
                {
                    repositorySourceRepository = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);
                }

                repository.LastUpdatedOn = now;
                repository.DefaultBranch = repositorySourceRepository.DefaultBranch;
                repository.Name = repositorySourceRepository.Name;
                repository.Id = repositorySourceRepository.Id;
                repository.Topics = repositorySourceRepository.Topics;

                repository.Dependencies = await ScrapeDependenciesAsync();

                if (repository.CreatedOn != repository.LastUpdatedOn)
                {
                    await repositoryManager.UpdateAsync(repository);
                }
                else
                {
                    await repositoryManager.CreateAsync(repository);
                }
            }

            async Task<List<RepositoryDependency>> ScrapeDependenciesAsync()
            {
                var sourceFileRegexes = dependencyScraperManagers.Select(dependencyManager => dependencyManager.SourceFileRegex);
                var sourceFiles = await repositorySourceManager.ReadFilesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repositorySourceRepository.DefaultBranch);

                var sourceFilesToRead = new List<string>();
                foreach (var sourceFile in sourceFiles)
                {
                    foreach (var sourceFileRegex in sourceFileRegexes)
                    {
                        if (sourceFileRegex.IsMatch(sourceFile.FullPath))
                        {
                            sourceFilesToRead.Add(sourceFile.FullPath);
                        }
                    }
                }

                // Read in the file content in bulk to get the files cached for the dependency managers to read
                await repositorySourceManager.GetMultipleFileContentsAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repositorySourceRepository.DefaultBranch, sourceFilesToRead).ConfigureAwait(false);

                var allDependencies = new List<RepositoryDependency>();

                foreach (var dependencyManager in dependencyScraperManagers)
                {
                    var dependencies = await dependencyManager.ReadAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repositorySourceRepository.DefaultBranch).ConfigureAwait(false);
                    allDependencies.AddRange(dependencies);
                }

                return allDependencies;
            }

            (string owner, string name) ParseRepositoryUrl()
            {
                var repositoryUri = new Uri(repositoryAnalysis.RepositoryUrl);
                var owner = repositoryUri.Segments[1].TrimEnd('/');
                var name = repositoryUri.Segments[2].TrimEnd('/');

                return (owner, name);
            }
        }
    }
}
