using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryManager
    {
        Task CreateAsync(Repository repository);
        Task<Repository> ReadAsync(string id);
        Task UpdateAsync(Repository repository);
        Task DeleteAsync(string id);
    }
}