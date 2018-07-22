using System;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// GitHub repository info needed to determine whether or not a new snapshot should be created
    /// </summary>
    public class RepositorySummary
    {
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// The id of the commit closest to but not after a provided datetime
        /// </summary>
        public string ClosestCommitId { get; set; }
        /// <summary>
        /// The pushed date of the commit closest to but not after a provided datetime
        /// </summary>
        public DateTime? ClosestCommitPushedDate { get; set; }
        /// <summary>
        /// The git tree id of the commit closes to but not after a provided datetime. Used to read repository content at the provided datetime
        /// </summary>
        public string ClosestCommitTreeId { get; set; }
        
    }
}
