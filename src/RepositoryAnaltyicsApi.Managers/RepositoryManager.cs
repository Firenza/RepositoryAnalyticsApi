using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositoryManager
    {
        private IRepositorySnapshotRepository repositorySnapshotRepository;
        private IRepositoryCurrentStateRepository repositoryCurrentStateRepository;
        private IRepositorySourceManager repositorySourceManager;

        public RepositoryManager(IRepositorySnapshotRepository repositorySnapshotRepository, IRepositoryCurrentStateRepository repositoryCurrentStateRepository, IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySnapshotRepository = repositorySnapshotRepository;
            this.repositoryCurrentStateRepository = repositoryCurrentStateRepository;
            this.repositorySourceManager = repositorySourceManager;
        }

        public async Task UpsertAsync(Repository repository, DateTime? asOf)
        {
            await repositoryCurrentStateRepository.UpsertAsync(repository.CurrentState).ConfigureAwait(false);

            // Get the commit related information for the repo at the specified point in time
            // Need this to properly set the window time range for which the snapshot information we are 
            // saving is valid for 
            var repoSummary = repositorySourceManager.ReadRepositorySummaryAsync(repository.CurrentState.Owner, repository.CurrentState.Name, repository.CurrentState.DefaultBranch, asOf);


            //// Save the snapshot portion
            //var repositorySnapshot = new RepositorySnapshot
            //{
            //    RepositoryCurrentStateId = repositoryCurrentState.Id,
            //    TakenOn = DateTime.Now,
            //    Dependencies = repository.Dependencies,
            //    // Going to also need to pass in the window range / commit id info to this method to save the state
            //    WindowStartCommitId = null,
            //    WindowEndsOn = null,
            //    WindowStartsOn = null,
            //    Id = null
            //};


            // Should probs do an upsert here

            await repositorySnapshotRepository.CreateAsync(repository.Snapshot).ConfigureAwait(false);
        }

        public async Task<Repository> ReadAsync(string id, DateTime? asOf)
        {
            return null;
        }

        public async Task<List<Repository>> SearchAsync(RepositorySearch repositorySearch)
        {
            return null;
            //return await repositoryRepository.SearchAsync(repositorySearch);
        }

    }
}
