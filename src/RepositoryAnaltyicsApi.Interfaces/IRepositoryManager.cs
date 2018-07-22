using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryManager
    {
        Task UpsertAsync(Repository repository, DateTime? asOf);
        Task<Repository> ReadAsync(string id, DateTime? asOf);
    }
}
