using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class IntervalCountAggregations
    {
        public DateTime? IntervalStart { get; set; }
        public DateTime? IntervalEnd { get; set; }
        public List<CountAggreation> CountAggreations { get; set; }
    }
}