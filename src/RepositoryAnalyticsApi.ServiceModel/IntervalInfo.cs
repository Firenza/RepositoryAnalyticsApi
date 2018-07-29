using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class IntervalInfo
    {
        public DateTime? IntervalStartTime { get; set; }
        public DateTime? IntervalEndTime { get; set; }
        public int? Intervals { get; set; }
    }
}
