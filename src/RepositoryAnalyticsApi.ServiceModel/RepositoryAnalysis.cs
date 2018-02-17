using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryAnalysis
    {
        public string RepositoryUrl { get; set; }
        /// <summary>
        /// Refresh all data even if there have been no updates in the source
        /// </summary>
        public bool ForceCompleteRefresh { get; set; }
        /// <summary>
        /// Optionally provide the last time the repository was updated to save a read call to fetch this data
        /// </summary>
        public DateTime? LastUpdatedOn { get; set; }
    }
}
