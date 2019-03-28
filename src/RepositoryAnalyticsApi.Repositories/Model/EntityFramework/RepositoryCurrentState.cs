using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryCurrentState
    {
        public int RepositoryCurrentStateId { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateTime? RepositoryCreatedOn { get; set; }
        public DateTime? RepositoryLastUpdatedOn { get; set; }
        public string DefaultBranch { get; set; }
        public bool? HasIssues { get; set; }
        public bool? HasProjects { get; set; }
        public bool? HasPullRequests { get; set; }
        public bool? ContinuousIntegration { get; set; }
        public bool? ContinuousDelivery { get; set; }
        public bool? ContinuousDeployment { get; set; }


        public ICollection<RepositoryTeam> Teams { get; set; }
        public ICollection<Topic> Topics { get; set; }
        public ICollection<RepositorySnapshot> RepositorySnapshots { get; set; }
    }
}
