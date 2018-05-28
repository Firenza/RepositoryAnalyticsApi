using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySnapshotManager
    {
        Task CreateAsync(RepositorySnapshot repository);
        Task<RepositorySnapshot> ReadAsync(string id);
        Task UpdateAsync(RepositorySnapshot repository);
        Task DeleteAsync(string id);
        Task<List<RepositorySnapshot>> SearchAsync(RepositorySearch repositorySearch);
    }
}