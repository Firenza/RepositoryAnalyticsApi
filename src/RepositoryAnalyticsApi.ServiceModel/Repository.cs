using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string DefaultBranch { get; set; }
        public RepositoryDevOpsIntegrations DevOpsIntegrations { get; set; }
        public IEnumerable<string> Topics { get; set; }
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }
        public IEnumerable<RepositoryTypeAndImplementations> TypesAndImplementations { get; set; }
    }
}
