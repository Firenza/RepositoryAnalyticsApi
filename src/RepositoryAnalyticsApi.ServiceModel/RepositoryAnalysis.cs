using System;
using System.ComponentModel.DataAnnotations;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryAnalysis
    {
        /// <summary>
        /// As of now this is the GitHub URL of the repostiory
        /// </summary>
        [Required]
        public string RepositoryId { get; set; }

        /// <summary>
        /// The date the repoistory was last updated in GitHub.  Can be provided to save a GitHub API call to get this information.
        /// </summary>
        public DateTime? RepositoryLastUpdatedOn { get; set; }

        /// <summary>
        /// Refresh all data even if there have been no updates in the source
        /// </summary>
        public bool ForceCompleteRefresh { get; set; }

        /// <summary>
        /// The point in time at which to do the analysis.  If not populated the current time is used.
        /// </summary>
        public DateTime?  AsOf { get; set; }
    }
}
