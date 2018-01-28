using System;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string DefaultBranch { get; set; }
    }
}
