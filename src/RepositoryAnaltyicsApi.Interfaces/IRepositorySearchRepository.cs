using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySearchRepository
    {
        Task<List<string>> SearchAsync(RepositorySearch repositorySearch);
    }
}
