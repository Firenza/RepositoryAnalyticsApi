using System;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryAnalysis
    {
        /// <summary>
        /// As of now this is the GitHub URL of the repostiory
        /// </summary>
        public string RepositoryId { get; set; }
        
        /// <summary>
        /// Refresh all data even if there have been no updates in the source
        /// </summary>
        public bool ForceCompleteRefresh { get; set; }

        /// <summary>
        /// The id of the commit that is closest to the AsOf time without being after it
        /// </summary>
        public string ClosestCommitId { get; set; }

        /// <summary>
        /// The time of the commit that is closest to the AsOf time without being after it
        /// </summary>
        public DateTime? ClosestCommitPushedOn { get; set; }

        /// <summary>
        /// The point in time at which to do the analysis.  If not populated the current time is used.
        /// </summary>
        public DateTime?  AsOf { get; set; }
    }
}
