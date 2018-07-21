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

        public RepositoryManager(IRepositorySnapshotRepository repositorySnapshotRepository, IRepositoryCurrentStateRepository repositoryCurrentStateRepository)
        {
            this.repositorySnapshotRepository = repositorySnapshotRepository;
            this.repositoryCurrentStateRepository = repositoryCurrentStateRepository;
        }

        public async Task UpsertAsync(Repository repository)
        {
            //// Map the current state stuff and update it
            //var repositoryCurrentState = new RepositoryCurrentState
            //{
            //    DefaultBranch = repository.DefaultBranch,
            //    DevOpsIntegrations = repository.DevOpsIntegrations,
            //    HasIssues = repository.HasIssues,
            //    HasProjects = repository.HasProjects,
            //    HasPullRequests = repository.HasPullRequests,
            //    RepositoryCreatedOn = repository.CreatedOn,
            //    RepositoryName = repository.Name,
            //    Id = repository.Id,
            //    Teams = repository.Teams,
            //    Topics = repository.Topics
            //};

            await repositoryCurrentStateRepository.UpsertAsync(repository.CurrentState).ConfigureAwait(false);

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
