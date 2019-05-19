using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryTypeAndImplementations
    {
        public string TypeName { get; set; }
        public IEnumerable<RepositoryImplementation> Implementations { get; set; }
    }
}
