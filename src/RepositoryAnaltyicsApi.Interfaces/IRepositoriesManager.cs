using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoriesManager
    {
        Task CreateAsync(Repository repository);
        Task<Repository> ReadAsync(string id);
        Task UpdateAsync(Repository repository);
        Task DeleteAsync(string id);
        Task<List<Repository>> SearchAsync(RepositorySearch repositorySearch);
    }
}