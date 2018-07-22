using System;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Information about a Repository at a specific point in time
    /// </summary>
    public class RepositorySourceSnapshot
    {
        /// <summary>
        /// The id of the commit closest to but not after a provided datetime
        /// </summary>
        public string ClosestCommitId { get; set; }
        /// <summary>
        /// The pushed date of the commit closest to but not after a provided datetime
        /// </summary>
        public DateTime? ClosestCommitPushedDate { get; set; }

        /// <summary>
        /// The committed date of the commit closest to but not after a provided datetime
        /// </summary>
        public DateTime ClosestCommitCommittedDate { get; set; }
        /// <summary>
        /// The git tree id of the commit closes to but not after a provided datetime. Used to read repository content at the provided datetime
        /// </summary>
        public string ClosestCommitTreeId { get; set; }
    }
}
