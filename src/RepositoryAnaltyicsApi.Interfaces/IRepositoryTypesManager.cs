using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryTypeManager
    {
        Task<List<CountAggregationResult>> ReadAllAsync();
    }
}
