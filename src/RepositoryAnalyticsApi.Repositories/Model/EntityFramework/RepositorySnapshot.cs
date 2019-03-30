using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositorySnapshot
    {
        public int RepositorySnapshotId { get; set; }

        /// <summary>
        /// The repository commit id corresponding to the WindowStart time
        /// </summary>
        public string WindowStartCommitId { get; set; }
        /// <summary>
        /// The point in time where the valid duration of this snapshot starts
        /// </summary>
        public DateTime? WindowStartsOn { get; set; }
        /// <summary>
        /// The point in time where the valid duration of this snapshot ends
        /// </summary>
        public DateTime? WindowEndsOn { get; set; }
        /// <summary>
        /// When the snapshot was created
        /// </summary>
        public DateTime? TakenOn { get; set; }
        /// <summary>
        /// Branch name used to compile the snapshot
        /// </summary>
        public string BranchUsed { get; set; }

        public ICollection<RepositoryDependency> Dependencies { get; set; }
        public ICollection<RepositoryTypeAndImplementations> TypesAndImplementations { get; set; }
        public ICollection<RepositoryFile> Files { get; set; }

        public int RepositoryCurrentStateId { get; set; }
        public RepositoryCurrentState RepositoryCurrentState { get; set; }
    }
}
