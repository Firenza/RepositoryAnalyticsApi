using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryCurrentStateRepository
    {
        Task<int?> UpsertAsync(RepositoryCurrentState repository);
        Task<RepositoryCurrentState> ReadAsync(string repositoryId);
    }
}
