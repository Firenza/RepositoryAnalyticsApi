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

        public async Task CreateAsync(string repositoryUrl)
        {
            var parsedRepoUrl = ParseRepositoryUrl();

            // First check to see if the repo has changed since it was last analyzed
            var repoSourceRead = repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);
            var repoRead = repositoryManager.ReadAsync(repositoryUrl);

            var repoSourceRepo = await repoSourceRead.ConfigureAwait(false);
            var repo = await repoRead.ConfigureAwait(false);

            if (repo == null)
            {
                var now = DateTime.Now;
                repo = new Repository();
                repo.CreatedOn = now;
            }

            if (repo.LastUpdatedOn < repoSourceRepo.LastUpdatedOn)
            {
                repo.LastUpdatedOn = DateTime.Now;
                repo.DefaultBranch = repoSourceRepo.DefaultBranch;
                repo.Name = repoSourceRepo.Name;
                repo.Id = repoSourceRepo.Id;
                repo.Topics = repoSourceRepo.Topics;

                repo.Dependencies = await ScrapeDependenciesAsync();

                await repositoryManager.CreateAsync(repo);
            }

            async Task<List<RepositoryDependency>> ScrapeDependenciesAsync()
            {
                var sourceFileRegexes = dependencyScraperManagers.Select(dependencyManager => dependencyManager.SourceFileRegex);
                var sourceFiles = await repositorySourceManager.ReadFilesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repoSourceRepo.DefaultBranch);

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
                repositorySourceManager.GetMultipleFileContents(parsedRepoUrl.owner, parsedRepoUrl.name, repoSourceRepo.DefaultBranch, sourceFilesToRead);

                var allDependencies = new List<RepositoryDependency>();

                foreach (var dependencyManager in dependencyScraperManagers)
                {
                    var dependencies = await dependencyManager.ReadAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repoSourceRepo.DefaultBranch).ConfigureAwait(false);
                    allDependencies.AddRange(dependencies);
                }

                return allDependencies;
            }

            (string owner, string name) ParseRepositoryUrl()
            {
                var repositoryUri = new Uri(repositoryUrl);
                var owner = repositoryUri.Segments[1].TrimEnd('/');
                var name = repositoryUri.Segments[2].TrimEnd('/');

                return (owner, name);
            }
        }
    }
}
