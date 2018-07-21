using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Repository information from source control (I.E. GitHub)
    /// </summary>
    public class RepositorySourceRepository
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime? PushedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DefaultBranchName { get; set; }
        public int projectCount { get; set; }
        public int issueCount { get; set; }
        public int pullRequestCount { get; set; }
        List<string> TopicNames { get; set; }
    }
}
