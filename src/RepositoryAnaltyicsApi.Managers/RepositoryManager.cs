using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryManager : IRepositoryManager
    {
        private IRepositorySnapshotRepository mongoRepositorySnapshotRepository;
        private IRepositorySnapshotRepository mySqlRepositorySnapshotRepository;
        private IRepositoryCurrentStateRepository mongoRepositoryCurrentStateRepository;
        private IRepositoryCurrentStateRepository mySqlRepositoryCurrentStateRepository;
        private IRepositorySourceManager repositorySourceManager;
        private IRepositorySearchRepository repositorySearchRepository;

        public RepositoryManager(
            IRepositorySnapshotRepository mongoRepositorySnapshotRepository,
            IRepositorySnapshotRepository mySqlRepositorySnapshotRepository,
            IRepositoryCurrentStateRepository mongoRepositoryCurrentStateRepository,
            IRepositoryCurrentStateRepository mySqlRepositoryCurrentStateRepository,
            IRepositorySourceManager repositorySourceManager, 
            IRepositorySearchRepository repositorySearchRepository
        )
        {
            this.mongoRepositorySnapshotRepository = mongoRepositorySnapshotRepository;
            this.mySqlRepositorySnapshotRepository = mySqlRepositorySnapshotRepository;
            this.mongoRepositoryCurrentStateRepository = mongoRepositoryCurrentStateRepository;
            this.mySqlRepositoryCurrentStateRepository = mySqlRepositoryCurrentStateRepository;
            this.repositorySourceManager = repositorySourceManager;
            this.repositorySearchRepository = repositorySearchRepository;
        }

        public async Task UpsertAsync(Repository repository, DateTime? asOf)
        {
            await mongoRepositoryCurrentStateRepository.UpsertAsync(repository.CurrentState).ConfigureAwait(false);
            var repositoryCurrentStateId = await mySqlRepositoryCurrentStateRepository.UpsertAsync(repository.CurrentState).ConfigureAwait(false);

            if (repository.Snapshot != null)
            {
                // Get the commit related information for the repo at the specified point in time
                // Need this to properly set the window time range for which the snapshot information we are 
                // saving is valid for 
                var repoSourceSnapshot = await repositorySourceManager.ReadRepositorySourceSnapshotAsync(repository.CurrentState.Owner, repository.CurrentState.Name, repository.CurrentState.DefaultBranch, asOf);

                repository.Snapshot.WindowStartCommitId = repoSourceSnapshot.ClosestCommitId;
                // Sometimes the pushed date is null, E.G. When a repository is renamed and the commit occured prior to the rename
                repository.Snapshot.WindowStartsOn = repoSourceSnapshot.ClosestCommitPushedDate ?? repoSourceSnapshot.ClosestCommitCommittedDate;

                // Do we really need to read all existing repository snapshots here?  Could probably get away with just reading
                // the first one before the asOf date and the first one after the asOf date but that can be left as a future
                // optimization
                var existingSnapshots = await mongoRepositorySnapshotRepository.ReadAllForParent(repository.CurrentState.Id);

                if (!existingSnapshots.Any())
                {
                    repository.Snapshot.WindowEndsOn = null;
                }
                else
                {
                    /*
                     * We need to figure out the following two things
                     * 
                     * 1) When should this snapshots window end?  
                     * 
                     * If there is a snapshot with a window starting at a later date then this
                     * later snapshots starting window time should be this snapshots ending window time
                     * 
                     * 2) Do we need to update the window end of an existing snapshot? 
                     * 
                     * If there an existing snapshot who's end date is later than the current snapshots start date 
                     * then we need to update this existing snapshots end date
                     */

                    var snapshotsWithLaterStartingDate = existingSnapshots.Where(snapshot => snapshot.WindowStartsOn > repository.Snapshot.WindowStartsOn);

                    if (snapshotsWithLaterStartingDate.Any())
                    {
                        var closestLaterStartingSnapshot = snapshotsWithLaterStartingDate.OrderBy(snapshot => snapshot.WindowStartsOn).First();

                        repository.Snapshot.WindowEndsOn = closestLaterStartingSnapshot.WindowStartsOn.Value.AddTicks(-1);
                    }

                    var snapshotsWithEarlierStartingDate = existingSnapshots.Where(snapshot => snapshot.WindowStartsOn < repository.Snapshot.WindowStartsOn);

                    if (snapshotsWithEarlierStartingDate.Any())
                    {
                        var closestEarlierStartingSnapshot = snapshotsWithEarlierStartingDate.OrderByDescending(snapshot => snapshot.WindowStartsOn).First();

                        closestEarlierStartingSnapshot.WindowEndsOn = repository.Snapshot.WindowStartsOn.Value.AddTicks(-1);

                        await mongoRepositorySnapshotRepository.UpsertAsync(closestEarlierStartingSnapshot).ConfigureAwait(false);
                        await mySqlRepositorySnapshotRepository.UpsertAsync(closestEarlierStartingSnapshot, repositoryCurrentStateId).ConfigureAwait(false);
                    }

                }

                await mongoRepositorySnapshotRepository.UpsertAsync(repository.Snapshot).ConfigureAwait(false);
                await mySqlRepositorySnapshotRepository.UpsertAsync(repository.Snapshot, repositoryCurrentStateId).ConfigureAwait(false);
            }
        }

        public async Task<Repository> ReadAsync(string id, DateTime? asOf)
        {
            return null;
        }

        public async Task<List<string>> SearchAsync(RepositorySearch repositorySearch)
        {
            return await repositorySearchRepository.SearchAsync(repositorySearch);
        }

    }
}
