using System;
using System.Collections.Generic;
namespace RepositoryAnalyticsApi.ServiceModel
{
    public class RepositorySearch
    {
        public string TypeName { get; set; }
        public string ImplementationName { get; set; }
        public List<(string Name, string Version, RangeSpecifier RangeSpecifier)> Dependencies { get; set; }
        public bool? HasContinuousDelivery { get; set; }
        public DateTime? AsOf { get; set; }
        public string Topic { get; set; }
        public string Team { get; set; }
    }
}