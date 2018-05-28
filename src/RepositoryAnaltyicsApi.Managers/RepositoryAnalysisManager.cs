using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryAnalysisManager : IRepositoryAnalysisManager
    {
        private IRepositoriesManager repositoryManager;
        private IRepositorySourceManager repositorySourceManager;
        private IEnumerable<IDependencyScraperManager> dependencyScraperManagers;
        private IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers;
        private IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver;

        public RepositoryAnalysisManager(IRepositoriesManager repositoryManager, IRepositorySourceManager repositorySourceManager, IEnumerable<IDependencyScraperManager> dependencyScraperManagers, IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers, IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver)
        {
            this.repositoryManager = repositoryManager;
            this.repositorySourceManager = repositorySourceManager;
            this.dependencyScraperManagers = dependencyScraperManagers;
            this.typeAndImplementationDerivers = typeAndImplementationDerivers;
            this.devOpsIntegrationsDeriver = devOpsIntegrationsDeriver;
        }

        public async Task CreateAsync(RepositoryAnalysis repositoryAnalysis)
        {
            var repository = await repositoryManager.ReadAsync(repositoryAnalysis.RepositoryUrl);
            Repository repositorySourceRepository = null;

            var now = DateTime.Now;
            bool repositoryAlreadyExists = true;

            if (repository == null)
            {
                repository = new Repository();
                repositoryAlreadyExists = false;
            }

            var parsedRepoUrl = ParseRepositoryUrl();

            var repositoryNeedsUpdating = false;

            if (repositoryAnalysis.ForceCompleteRefresh)
            {
                repositoryNeedsUpdating = true;
            }
            else if(repositoryAnalysis.LastUpdatedOn.HasValue)
            {
                repositoryNeedsUpdating = repository.AnalysisLastUpdatedOn < repositoryAnalysis.LastUpdatedOn.Value;
            }
            else
            {
                repositorySourceRepository = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);

                repositoryNeedsUpdating = repository.AnalysisLastUpdatedOn < repositorySourceRepository.LastUpdatedOn;
            }

            if (repositoryNeedsUpdating)
            {
                if (repositorySourceRepository == null)
                {
                    repositorySourceRepository = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);
                }

                repository.AnalysisLastUpdatedOn = now;
                repository.CreatedOn = repositorySourceRepository.CreatedOn;
                repository.LastUpdatedOn = repositorySourceRepository.LastUpdatedOn;
                repository.DefaultBranch = repositorySourceRepository.DefaultBranch;
                repository.Name = repositorySourceRepository.Name;
                repository.Id = repositorySourceRepository.Id;
                repository.Topics = repositorySourceRepository.Topics;
                repository.Teams = repositorySourceRepository.Teams;

                repository.Dependencies = await ScrapeDependenciesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repository.DefaultBranch);
                repository.TypesAndImplementations = await ScrapeRepositoryTypeAndImplementation(repository, parsedRepoUrl.owner);
                repository.DevOpsIntegrations = await ScrapeDevOpsIntegrations(repository.Name);

                if (repositoryAlreadyExists)
                {
                    await repositoryManager.UpdateAsync(repository);
                }
                else
                {
                    await repositoryManager.CreateAsync(repository);
                }
            }

            (string owner, string name) ParseRepositoryUrl()
            {
                var repositoryUri = new Uri(repositoryAnalysis.RepositoryUrl);
                var owner = repositoryUri.Segments[1].TrimEnd('/');
                var name = repositoryUri.Segments[2].TrimEnd('/');

                return (owner, name);
            }
        }

        private async Task<RepositoryDevOpsIntegrations> ScrapeDevOpsIntegrations(string repositoryName)
        {
            if (devOpsIntegrationsDeriver != null)
            {
                var devOpsIntegrations = await devOpsIntegrationsDeriver.DeriveIntegrationsAsync(repositoryName);

                return devOpsIntegrations;
            }
            else
            {
                return null;
            }
        }


        private async Task<List<RepositoryDependency>> ScrapeDependenciesAsync(string owner, string name, string defaultBranch)
        {
            var allDependencies = new List<RepositoryDependency>();

            var sourceFileRegexes = dependencyScraperManagers.Select(dependencyManager => dependencyManager.SourceFileRegex);
            var sourceFiles = await repositorySourceManager.ReadFilesAsync(owner, name, defaultBranch);

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

            if (sourceFilesToRead.Any())
            {
                // Read in the file content in bulk to get the files cached for the dependency managers to read
                await repositorySourceManager.GetMultipleFileContentsAsync(owner, name, defaultBranch, sourceFilesToRead).ConfigureAwait(false);

                foreach (var dependencyManager in dependencyScraperManagers)
                {
                    var dependencies = await dependencyManager.ReadAsync(owner, name, defaultBranch).ConfigureAwait(false);
                    allDependencies.AddRange(dependencies);
                }
            }

            return allDependencies;
        }

        private async Task<IEnumerable<RepositoryTypeAndImplementations>> ScrapeRepositoryTypeAndImplementation(Repository repository, string owner)
        {
            var typesAndImplementations = new List<RepositoryTypeAndImplementations>();

            var readFileContentAsync = new Func<string, Task<string>>(async (fullFilePath) =>
                await repositorySourceManager.ReadFileContentAsync(owner, repository.Name, fullFilePath).ConfigureAwait(false)
            );

            var readFilesAsync = new Func<Task<List<RepositoryFile>>>(async () =>
                await repositorySourceManager.ReadFilesAsync(owner, repository.Name, repository.DefaultBranch).ConfigureAwait(false)
            );

            foreach (var typeAndImplementationDeriver in typeAndImplementationDerivers)
            {
                var typeAndImplementationInfo = await typeAndImplementationDeriver.DeriveImplementationAsync(repository.Dependencies, readFilesAsync, repository.Topics, repository.Name, readFileContentAsync);

                if (typeAndImplementationInfo != null)
                {
                    typesAndImplementations.Add(typeAndImplementationInfo);
                }
            }

            return typesAndImplementations;
        }
    }
}