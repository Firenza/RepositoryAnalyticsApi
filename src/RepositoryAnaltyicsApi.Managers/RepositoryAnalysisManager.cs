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
        private IRepositorySnapshotManager repositorySnapshotManager;
        private IRepositorySourceManager repositorySourceManager;
        private IEnumerable<IDependencyScraperManager> dependencyScraperManagers;
        private IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers;
        private IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver;

        public RepositoryAnalysisManager(IRepositorySnapshotManager repositoryManager, IRepositorySourceManager repositorySourceManager, IEnumerable<IDependencyScraperManager> dependencyScraperManagers, IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers, IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver)
        {
            this.repositorySnapshotManager = repositoryManager;
            this.repositorySourceManager = repositorySourceManager;
            this.dependencyScraperManagers = dependencyScraperManagers;
            this.typeAndImplementationDerivers = typeAndImplementationDerivers;
            this.devOpsIntegrationsDeriver = devOpsIntegrationsDeriver;
        }

        public async Task CreateAsync(RepositoryAnalysis repositoryAnalysis)
        {
            /* If we are doing an analysis on the present state of things, check to see if we already have a snapshot saved which
             * reflects the current state of the repository.  
             * 
             * 1) Read in the lastest snapshot for the given repository
             * 2) Read in the last time the repository was updated in GitHub (if not already available)
             * 3) If the start window time for the most recent snapshot matches the last time the repository was updated then do nothing
             * 4) If .... Is older than the last time the repository was updated then we need to take a new snapshot
             *
             * Would be better to use a commit ID instead of a datetime as the datetimes will always be based on a commit anyway.  Could just update the read source repo
             * graphql call to return the last commit id and the orchestrator could send that in ont his request
            */

            /* If we are doing a snapshop of the past we will do the following
             * 
             * 1) Read in the snapshot that has a window matching the specified AsOf time (S1)
             * 2) Get the commit Id corresponding to the closest commit before the AsOf time (C5)
             * 3) Compare commitId's from 1) and 2) and if they are the same then do nothing
             * 4) Compare .... are different (C2 != C5) then create a new snapshot with a time window of the 2) commitId date time and the end window time of the matched snapshot
             * 5) Update existing snapshot by moving the end date of the matched snapshot back in time to be one tick before the 2) commit ID date time
             * 
             */

            DateTime? newSnapshotWindowStart = repositoryAnalysis.ClosestCommitPushedOn;
            string asOfDateRepositoryClosestCommitId = null;
            bool createNewSnapshot = false;

            var parsedRepoUrl = ParseRepositoryUrl();

            // Figure out if a new snapshot is needed
            var existingRepositorySnapshot = await repositorySnapshotManager.ReadAsync(repositoryAnalysis.RepositoryId);

            if (existingRepositorySnapshot == null)
            {
                createNewSnapshot = true;
            }
            else
            {
                var existingSnapShotStartCommitId = existingRepositorySnapshot.WindowStartCommitId;
                asOfDateRepositoryClosestCommitId = repositoryAnalysis.ClosestCommitId;

                if (string.IsNullOrWhiteSpace(asOfDateRepositoryClosestCommitId))
                {
                    // Make API call to get this
                    var repositorySummary = await repositorySourceManager.ReadRepositoriesAsync(parsedRepoUrl.owner, 1, null, repositoryAnalysis.AsOf);
                    asOfDateRepositoryClosestCommitId = repositorySummary.Results.First().ClosestCommitId;
                    newSnapshotWindowStart = repositorySummary.Results.First().ClosestCommitPushedDate;
                }

                createNewSnapshot = existingSnapShotStartCommitId != asOfDateRepositoryClosestCommitId;
            }
           

            if (createNewSnapshot)
            {
                // We need to create a new snapshot
                if (!newSnapshotWindowStart.HasValue)
                {
                    var repositorySummary = await repositorySourceManager.ReadRepositoriesAsync(parsedRepoUrl.owner, 1, null, repositoryAnalysis.AsOf);
                    asOfDateRepositoryClosestCommitId = repositorySummary.Results.First().ClosestCommitId;
                    newSnapshotWindowStart = repositorySummary.Results.First().ClosestCommitPushedDate;
                }

                // Set all the new snapshot info
                var newSnapshot = new RepositorySnapshot();

                newSnapshot.TakenOn = DateTime.Now;
                newSnapshot.WindowStartCommitId = asOfDateRepositoryClosestCommitId;
                newSnapshot.WindowStartsOn = newSnapshotWindowStart;
                newSnapshot.WindowEndsOn = existingRepositorySnapshot?.WindowEndsOn;
                newSnapshot.DefaultBranch = existingRepositorySnapshot.DefaultBranch;
                newSnapshot.RepositoryName = existingRepositorySnapshot.RepositoryName;
                //newSnapshot.Topics = repositorySnapshot.Topics;
                //newSnapshot.Teams = repositorySnapshot.Teams;

                newSnapshot.Dependencies = await ScrapeDependenciesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, existingRepositorySnapshot.DefaultBranch);
                newSnapshot.TypesAndImplementations = await ScrapeRepositoryTypeAndImplementation(existingRepositorySnapshot, parsedRepoUrl.owner);
                newSnapshot.DevOpsIntegrations = await ScrapeDevOpsIntegrations(existingRepositorySnapshot.RepositoryName);

                // Create the new snapshot
                await repositorySnapshotManager.CreateAsync(newSnapshot);

                if (existingRepositorySnapshot != null)
                {
                    // Update the existing snapshot to end right before the new one starts
                    existingRepositorySnapshot.WindowEndsOn = newSnapshotWindowStart.Value.AddTicks(-1);

                    await repositorySnapshotManager.UpdateAsync(existingRepositorySnapshot);
                }
            }


            (string owner, string name) ParseRepositoryUrl()
            {
                var repositoryUri = new Uri(repositoryAnalysis.RepositoryId);
                var owner = repositoryUri.Segments[1].TrimEnd('/');
                var name = repositoryUri.Segments[2].TrimEnd('/');

                return (owner, name);
            }

            //var now = DateTime.Now;
            //bool repositorySnapshotAlreadyExists = true;

            //if (existingRepositorySnapshot == null)
            //{
            //    existingRepositorySnapshot = new RepositorySnapshot();
            //    repositorySnapshotAlreadyExists = false;
            //}

            //var parsedRepoUrl = ParseRepositoryUrl();

            //var repositoryNeedsUpdating = false;

            //if (repositoryAnalysis.ForceCompleteRefresh)
            //{
            //    repositoryNeedsUpdating = true;
            //}
            //else if (repositoryAnalysis.SourceRepositoryLastUpdatedOn.HasValue)
            //{
            //    repositoryNeedsUpdating = existingRepositorySnapshot.TakenOn < repositoryAnalysis.SourceRepositoryLastUpdatedOn.Value;
            //}
            //else
            //{
            //    repositorySnapshot = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);

            //    repositoryNeedsUpdating = existingRepositorySnapshot.TakenOn < repositorySnapshot.LastUpdatedOn;
            //}

            //if (repositoryNeedsUpdating)
            //{
            //    if (repositorySnapshot == null)
            //    {
            //        repositorySnapshot = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.owner, parsedRepoUrl.name);
            //    }

            //    existingRepositorySnapshot.TakenOn = now;
            //    existingRepositorySnapshot.RepositoryCreatedOn = repositorySnapshot.RepositoryCreatedOn;
            //    existingRepositorySnapshot.LastUpdatedOn = repositorySnapshot.LastUpdatedOn;
            //    existingRepositorySnapshot.DefaultBranch = repositorySnapshot.DefaultBranch;
            //    existingRepositorySnapshot.Repository = repositorySnapshot.Repository;
            //    existingRepositorySnapshot.Id = repositorySnapshot.Id;
            //    existingRepositorySnapshot.Topics = repositorySnapshot.Topics;
            //    existingRepositorySnapshot.Teams = repositorySnapshot.Teams;

            //    existingRepositorySnapshot.Dependencies = await ScrapeDependenciesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, existingRepositorySnapshot.DefaultBranch);
            //    existingRepositorySnapshot.TypesAndImplementations = await ScrapeRepositoryTypeAndImplementation(existingRepositorySnapshot, parsedRepoUrl.owner);
            //    existingRepositorySnapshot.DevOpsIntegrations = await ScrapeDevOpsIntegrations(existingRepositorySnapshot.Repository);

            //    if (repositorySnapshotAlreadyExists)
            //    {
            //        await repositorySnapshotManager.UpdateAsync(existingRepositorySnapshot);
            //    }
            //    else
            //    {
            //        await repositorySnapshotManager.CreateAsync(existingRepositorySnapshot);
            //    }
            //}

            //(string owner, string name) ParseRepositoryUrl()
            //{
            //    var repositoryUri = new Uri(repositoryAnalysis.RepositoryId);
            //    var owner = repositoryUri.Segments[1].TrimEnd('/');
            //    var name = repositoryUri.Segments[2].TrimEnd('/');

            //    return (owner, name);
            //}
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

        private async Task<IEnumerable<RepositoryTypeAndImplementations>> ScrapeRepositoryTypeAndImplementation(RepositorySnapshot repository, string owner)
        {
            var typesAndImplementations = new List<RepositoryTypeAndImplementations>();

            var readFileContentAsync = new Func<string, Task<string>>(async (fullFilePath) =>
                await repositorySourceManager.ReadFileContentAsync(owner, repository.RepositoryName, fullFilePath).ConfigureAwait(false)
            );

            var readFilesAsync = new Func<Task<List<RepositoryFile>>>(async () =>
                await repositorySourceManager.ReadFilesAsync(owner, repository.RepositoryName, repository.DefaultBranch).ConfigureAwait(false)
            );

            foreach (var typeAndImplementationDeriver in typeAndImplementationDerivers)
            {
                var typeAndImplementationInfo = await typeAndImplementationDeriver.DeriveImplementationAsync(repository.Dependencies, readFilesAsync, repository.Topics, repository.RepositoryName, readFileContentAsync);

                if (typeAndImplementationInfo != null)
                {
                    typesAndImplementations.Add(typeAndImplementationInfo);
                }
            }

            return typesAndImplementations;
        }
    }
}
