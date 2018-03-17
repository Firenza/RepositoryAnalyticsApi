using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoriesRepository
    {
        Task CreateAsync(Repository repository);
        Task<Repository> ReadAsync(string id);
        Task UpdateAsync(Repository repository);
        Task DeleteAsync(string id);
        Task<List<Repository>> SearchAsync(string typeName, string implementationName, List<(string Name, string Version)> dependencies);
    }
}
