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
    }
}
