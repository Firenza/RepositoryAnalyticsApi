using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireDependenciesAccess
    {
        IEnumerable<RepositoryDependency> Dependencies { get; set; }
    }
}
