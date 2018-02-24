using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryTypeAndImplementations
    {
        public string TypeName { get; set; }
        public IEnumerable<RepositoryImplementation> Implementations { get; set; }
    }
}
