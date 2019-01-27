using System;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class RepositorySnapshot
    {
        public static RepositorySnapshot MapFrom(ServiceModel.RepositorySnapshot repositorySnapshot, int repositoryCurrentStateId)
        {
            return new RepositorySnapshot
            {
                RepositoryCurrentStateId = repositoryCurrentStateId,
                WindowStartCommitId = repositorySnapshot.WindowStartCommitId,
                WindowStartsOn = repositorySnapshot.WindowStartsOn,
                WindowEndsOn = repositorySnapshot.WindowEndsOn,
                TakenOn = repositorySnapshot.TakenOn,
                BranchUsed = repositorySnapshot.BranchUsed
            };
        }

        public int Id { get; set; }

        public int RepositoryCurrentStateId { get; set; }

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
    }
}
