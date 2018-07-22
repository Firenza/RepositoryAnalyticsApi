using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryImplementationsManager
    {
        Task<List<IntervalCountAggregations>> SearchAsync(string typeName, DateTime? intervalStartTime, DateTime? intervalEndTime, int? intervals);
    }
}