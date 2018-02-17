using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositoryDependencySearchResult
    {
        public RepositoryDependency RepositoryDependency { get; set; }
        public int? Count { get; set; }
    }
}
