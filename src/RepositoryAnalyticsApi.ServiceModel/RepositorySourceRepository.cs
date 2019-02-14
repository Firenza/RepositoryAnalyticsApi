using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Repository information from source control (I.E. GitHub)
    /// </summary>
    [Serializable]
    public class RepositorySourceRepository
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime? PushedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DefaultBranchName { get; set; }
        public List<string> BranchNames { get; set; }
        public int ProjectCount { get; set; }
        public int IssueCount { get; set; }
        public int PullRequestCount { get; set; }
        public List<string> TopicNames { get; set; }
        public List<RepositoryTeam> Teams { get; set; }
    }
}
