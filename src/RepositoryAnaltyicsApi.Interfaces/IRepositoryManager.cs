using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryManager
    {
        Task UpsertAsync(Repository repository);
        Task<Repository> ReadAsync(string id, DateTime? asOf);
    }
}
