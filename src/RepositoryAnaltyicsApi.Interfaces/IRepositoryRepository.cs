using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryRepository
    {
        Task CreateAsync(Repository repository);
        Task<Repository> ReadAsync(string id);
        Task UpdateAsync(Repository repository);
        Task DeleteAsync(string id);
    }
}
