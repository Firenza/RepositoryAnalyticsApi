using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryRepository
    {
        Task<List<Repository>> ReadMultipleAsync(DateTime? asOf, int? page, int? pageSize);
        Task<Repository> ReadAsync(string repositoryId, DateTime? asOf);
    }
}
