using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyRepository
    {
        Task<List<string>> SearchNamesAsync(string name, DateTime? asOf);
        Task<List<RepositoryDependencySearchResult>> SearchAsync(string name, DateTime? asOf);
    }
}
