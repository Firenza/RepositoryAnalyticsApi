﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Repository data that is not tracked over time by GitHub.  E.G. You can't get the teams assigned to a repository last month.
    /// </summary>
    public class RepositoryCurrentState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateTime? RepositoryCreatedOn { get; set; }
        public DateTime? RepositoryLastUpdatedOn { get; set; }
        public string DefaultBranch { get; set; }
        public bool? HasIssues { get; set; }
        public bool? HasProjects { get; set; }
        public bool? HasPullRequests { get; set; }
        public List<string> Teams { get; set; }
        public RepositoryDevOpsIntegrations DevOpsIntegrations { get; set; }
        public IEnumerable<string> Topics { get; set; }
    }
}