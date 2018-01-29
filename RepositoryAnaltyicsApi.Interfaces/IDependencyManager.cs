using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyManager
    {
        List<RepositoryDependency> Read(string repositoryId);
    }
}
