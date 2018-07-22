using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySnapshotRepository
    {
        Task UpsertAsync(RepositorySnapshot snapshot);
        Task<RepositorySnapshot> ReadAsync(string id);
        Task DeleteAsync(string id);
        Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId);
        Task<List<RepositorySnapshot>> SearchAsync(RepositorySearch repositorySearch);
    }
}
