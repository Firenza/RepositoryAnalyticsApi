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
        private IRepositoryManager repositoryManager;
        private IRepositorySourceManager repositorySourceManager;

        private IEnumerable<IDependencyScraperManager> dependencyScraperManagers;
        private IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers;
        private IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver;

        public RepositoryAnalysisManager(IRepositoryManager repositoryManager, IRepositorySourceManager repositorySourceManager, IEnumerable<IDependencyScraperManager> dependencyScraperManagers, IEnumerable<IDeriveRepositoryTypeAndImplementations> typeAndImplementationDerivers, IDeriveRepositoryDevOpsIntegrations devOpsIntegrationsDeriver)
        {
            this.repositoryManager = repositoryManager;
            this.repositorySourceManager = repositorySourceManager;
            this.dependencyScraperManagers = dependencyScraperManagers;
            this.typeAndImplementationDerivers = typeAndImplementationDerivers;
            this.devOpsIntegrationsDeriver = devOpsIntegrationsDeriver;
        }

        public async Task CreateAsync(RepositoryAnalysis repositoryAnalysis)
        {

            var parsedRepoUrl = ParseRepositoryUrl();

            var files = await repositorySourceManager.ReadFilesAsync(parsedRepoUrl.Owner, parsedRepoUrl.Name, "master", repositoryAnalysis.AsOf);

            if (!repositoryAnalysis.AsOf.HasValue)
            {
                


                /* If we are doing an analysis on the present state of things, check to see if we already have a snapshot saved which
               * reflects the current state of the repository.  
               * 
               * 1) Read in the latest Repository information we have stored
               * 2) Read in the last time the repository was updated in GitHub (if not already available)
               * 3) If the start window time for the most recent snapshot matches the last time the repository was updated then do nothing
               * 4) If .... Is older than the last time the repository was updated then we need to take a new snapshot
               *
               * Would be better to use a commit ID instead of a datetime as the datetimes will always be based on a commit anyway.  Could just update the read source repo
               * graphql call to return the last commit id and the orchestrator could send that in ont his request
              */
                
                DateTime? repositoryLastUpdatedOn = null;

                if (repositoryAnalysis.RepositoryLastUpdatedOn.HasValue)
                {
                    repositoryLastUpdatedOn = repositoryAnalysis.RepositoryLastUpdatedOn.Value;
                }
                else
                {
                    var repositorySummary = await repositorySourceManager.ReadRepositorySummaryAsync(parsedRepoUrl.Owner, parsedRepoUrl.Name, "master", null).ConfigureAwait(false);

                    repositoryLastUpdatedOn = repositorySummary.UpdatedAt;
                }

                var repository = await repositoryManager.ReadAsync(repositoryAnalysis.RepositoryId, null).ConfigureAwait(false);

                if (repository == null || repositoryLastUpdatedOn > repository.CurrentState.RepositoryLastUpdatedOn)
                {
           
                    // Do repository summary call to get the commit Id of the latest commit and the date that commit was pushed for the snapshot
                    // populate the snapshot date with the corresponding manager calls (E.G. ScrapeDependenciesAsync) 
                    // Do full repository read to get all the current state stuff (including calls to get derived data like devops integrations)
                    var sourceRepository = await repositorySourceManager.ReadRepositoryAsync(parsedRepoUrl.Owner, parsedRepoUrl.Name);

                    var repositoryCurrentState = new RepositoryCurrentState();
                    repositoryCurrentState.DefaultBranch = sourceRepository.DefaultBranchName;
                    repositoryCurrentState.HasIssues = sourceRepository.IssueCount > 0;
                    repositoryCurrentState.HasProjects = sourceRepository.ProjectCount > 0;
                    repositoryCurrentState.HasPullRequests = sourceRepository.PullRequestCount > 0;
                    repositoryCurrentState.RepositoryCreatedOn = sourceRepository.CreatedAt;
                    repositoryCurrentState.RepositoryLastUpdatedOn = sourceRepository.PushedAt;
                    repositoryCurrentState.Name = sourceRepository.Name;
                    repositoryCurrentState.Teams = sourceRepository.Teams;
                    repositoryCurrentState.Topics = sourceRepository.TopicNames;
                    repositoryCurrentState.DevOpsIntegrations = await ScrapeDevOpsIntegrations(repositoryCurrentState.Name);
    

                }

            }
            else
            {
                /* If we are doing a snapshop of the past we will do the following
                * 
                * 1) Read in the snapshot that has a window matching the specified AsOf time (S1)
                * 2) Get the commit Id corresponding to the closest commit before the AsOf time (C5)
                * 3) Compare commitId's from 1) and 2) and if they are the same then do nothing
                * 4) Compare .... are different (C2 != C5) then create a new snapshot with a time window of the 2) commitId date time and the end window time of the matched snapshot
                * 5) Update existing snapshot by moving the end date of the matched snapshot back in time to be one tick before the 2) commit ID date time
                * 
                */

            }



            //DateTime? newSnapshotWindowStart = repositoryAnalysis.ClosestCommitPushedOn;
            //string asOfDateRepositoryClosestCommitId = null;
            //bool createNewSnapshot = false;

            ////var parsedRepoUrl = ParseRepositoryUrl();

            //// Figure out if a new snapshot is needed
            //var existingRepositorySnapshot = await repositoryManager.ReadAsync(repositoryAnalysis.RepositoryId);

            //if (existingRepositorySnapshot == null)
            //{
            //    createNewSnapshot = true;
            //}
            //else
            //{
            //    var existingSnapShotStartCommitId = existingRepositorySnapshot.WindowStartCommitId;
            //    asOfDateRepositoryClosestCommitId = repositoryAnalysis.ClosestCommitId;

            //    if (string.IsNullOrWhiteSpace(asOfDateRepositoryClosestCommitId))
            //    {
            //        // Make API call to get this
            //        var repositorySummary = await repositorySourceManager.ReadRepositorySummaryAsync(parsedRepoUrl.Owner, parsedRepoUrl.Name, repositoryAnalysis.AsOf);
            //        asOfDateRepositoryClosestCommitId = repositorySummary.ClosestCommitId;
            //        newSnapshotWindowStart = repositorySummary.ClosestCommitPushedDate;
            //    }

            //    createNewSnapshot = existingSnapShotStartCommitId != asOfDateRepositoryClosestCommitId;
            //}
           

            //if (createNewSnapshot)
            //{
            //    // We need to create a new snapshot
            //    if (!newSnapshotWindowStart.HasValue)
            //    {
            //        var repositorySummary = await repositorySourceManager.ReadRepositorySummaryAsync(parsedRepoUrl.owner, parsedRepoUrl.name, repositoryAnalysis.AsOf);
            //        asOfDateRepositoryClosestCommitId = repositorySummary.ClosestCommitId;
            //        newSnapshotWindowStart = repositorySummary.ClosestCommitPushedDate;
            //    }

            //    // Set all the new snapshot info
            //    var newSnapshot = new RepositorySnapshot();

            //    newSnapshot.TakenOn = DateTime.Now;
            //    newSnapshot.WindowStartCommitId = asOfDateRepositoryClosestCommitId;
            //    newSnapshot.WindowStartsOn = newSnapshotWindowStart;
            //    newSnapshot.WindowEndsOn = existingRepositorySnapshot?.WindowEndsOn;
            //    newSnapshot.DefaultBranch = existingRepositorySnapshot.DefaultBranch;
            //    newSnapshot.RepositoryName = existingRepositorySnapshot.RepositoryName;
            //    //newSnapshot.Topics = repositorySnapshot.Topics;
            //    //newSnapshot.Teams = repositorySnapshot.Teams;

            //    newSnapshot.Dependencies = await ScrapeDependenciesAsync(parsedRepoUrl.owner, parsedRepoUrl.name, existingRepositorySnapshot.DefaultBranch);
            //    newSnapshot.TypesAndImplementations = await ScrapeRepositoryTypeAndImplementation(existingRepositorySnapshot, parsedRepoUrl.owner);
            //    newSnapshot.DevOpsIntegrations = await ScrapeDevOpsIntegrations(existingRepositorySnapshot.RepositoryName);

            //    // Create the new snapshot
            //    await repositoryManager.CreateAsync(newSnapshot);

            //    if (existingRepositorySnapshot != null)
            //    {
            //        // Update the existing snapshot to end right before the new one starts
            //        existingRepositorySnapshot.WindowEndsOn = newSnapshotWindowStart.Value.AddTicks(-1);

            //        await repositoryManager.UpdateAsync(existingRepositorySnapshot);
            //    }
            //}


            (string Owner, string Name) ParseRepositoryUrl()
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


        private async Task<List<RepositoryDependency>> ScrapeDependenciesAsync(string owner, string name, string defaultBranch, DateTime? asOf = null)
        {
            var allDependencies = new List<RepositoryDependency>();

            var sourceFileRegexes = dependencyScraperManagers.Select(dependencyManager => dependencyManager.SourceFileRegex);
            var sourceFiles = await repositorySourceManager.ReadFilesAsync(owner, name, defaultBranch, asOf);

            // Get the files that all the dependency scrapers need so we can read them all in one shot and have them
            // cached for each dependency scraper
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
                // Get all the file contents that will be needed read and in cache
                await repositorySourceManager.GetMultipleFileContentsAsync(owner, name, defaultBranch, sourceFilesToRead, asOf).ConfigureAwait(false);

                foreach (var dependencyManager in dependencyScraperManagers)
                {
                    var dependencies = await dependencyManager.ReadAsync(owner, name, defaultBranch, asOf).ConfigureAwait(false);
                    allDependencies.AddRange(dependencies);
                }
            }

            return allDependencies;
        }

        //private async Task<IEnumerable<RepositoryTypeAndImplementations>> ScrapeRepositoryTypeAndImplementation(RepositorySnapshot repository, string owner)
        //{
        //    var typesAndImplementations = new List<RepositoryTypeAndImplementations>();

        //    var readFileContentAsync = new Func<string, Task<string>>(async (fullFilePath) =>
        //        await repositorySourceManager.ReadFileContentAsync(owner, repository.RepositoryName, fullFilePath).ConfigureAwait(false)
        //    );

        //    var readFilesAsync = new Func<Task<List<RepositoryFile>>>(async () =>
        //        await repositorySourceManager.ReadFilesAsync(owner, repository.RepositoryName, repository.DefaultBranch).ConfigureAwait(false)
        //    );

        //    foreach (var typeAndImplementationDeriver in typeAndImplementationDerivers)
        //    {
        //        var typeAndImplementationInfo = await typeAndImplementationDeriver.DeriveImplementationAsync(repository.Dependencies, readFilesAsync, repository.Topics, repository.RepositoryName, readFileContentAsync);

        //        if (typeAndImplementationInfo != null)
        //        {
        //            typesAndImplementations.Add(typeAndImplementationInfo);
        //        }
        //    }

        //    return typesAndImplementations;
        //}
    }
}
