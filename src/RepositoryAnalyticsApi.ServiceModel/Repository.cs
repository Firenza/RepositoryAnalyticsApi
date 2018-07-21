using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// A DTO representing a GitHub repository
    /// </summary>
    public class Repository
    {
        public RepositoryCurrentState CurrentState { get; set; }
        public RepositorySnapshot Snapshot { get; set; }

        //public string Id { get; set; }
        //public string Name { get; set; }
        //public DateTime? CreatedOn { get; set; }
        //public DateTime? LastUpdatedOn { get; set; }
        //public string DefaultBranch { get; set; }
        //public bool? HasIssues { get; set; }
        //public bool? HasProjects { get; set; }
        //public bool? HasPullRequests { get; set; }
        //public List<string> Teams { get; set; }
        //public IEnumerable<string> Topics { get; set; }
        //public RepositoryDevOpsIntegrations DevOpsIntegrations { get; set; }
        //public IEnumerable<RepositoryDependency> Dependencies { get; set; }
    }
}
