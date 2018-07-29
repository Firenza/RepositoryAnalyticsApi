using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryImplementationsRepository
    {
        Task<IntervalCountAggregations> SearchAsync(RepositorySearch repositorySearch, DateTime? createdOnOrAfter, DateTime? createdOnOrBefore);
    }
}