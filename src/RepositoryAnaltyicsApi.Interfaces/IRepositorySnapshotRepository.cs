using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySnapshotRepository
    {
        Task UpsertAsync(RepositorySnapshot snapshot, int? repositoryCurrentStateId = null);
        Task<RepositorySnapshot> ReadAsync(string repositoryId, DateTime? asOf);
        Task DeleteAsync(string id);
        Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId);
    }
}
