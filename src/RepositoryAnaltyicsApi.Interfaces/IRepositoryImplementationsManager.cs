using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryImplementationsManager
    {
        Task<List<IntervalCountAggregations>> SearchAsync(RepositorySearch repositorySearch, IntervalInfo intervalInfo);
    }
}