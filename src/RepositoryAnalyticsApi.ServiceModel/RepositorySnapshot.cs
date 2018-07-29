using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Derived data corresponding to a given window of time for a Repository. Combined with RepositoryCurrentState at search time to get all repository data.
    /// </summary>
    public class RepositorySnapshot
    {
        public string RepositoryCurrentStateId { get; set; }

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

        public IEnumerable<RepositoryDependency> Dependencies { get; set; }
        public IEnumerable<RepositoryTypeAndImplementations> TypesAndImplementations { get; set; }
    }
}