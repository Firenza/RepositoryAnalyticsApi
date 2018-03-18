using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositorySearch
    {
        public string TypeName { get; set; }
        public string ImplementationName { get; set; }
        public List<(string Name, string Version)> Dependencies { get; set; }
        public bool? HasContinuousDelivery { get; set; }
    }
}
