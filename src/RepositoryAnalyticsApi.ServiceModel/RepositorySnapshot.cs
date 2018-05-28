using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// A representation of a GitHub repository and some drived data corresponding to a given window of time
    /// </summary>
    public class RepositorySnapshot
    {
        public string Id { get; set; }
        public string RepositoryName { get; set; }
        /// <summary>
        /// When the repository was created in GitHub
        /// </summary>
        public DateTime? RepositoryCreatedOn { get; set; }
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
        public string DefaultBranch { get; set; }
        public bool? HasIssues { get; set; }
        public bool? HasProjects { get; set; }
        public bool? HasPullRequests { get; set; }
        public List<string> Teams { get; set; }
        public RepositoryDevOpsIntegrations DevOpsIntegrations { get; set; }
        public IEnumerable<string> Topics { get; set; }
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }
        public IEnumerable<RepositoryTypeAndImplementations> TypesAndImplementations { get; set; }
    }
}