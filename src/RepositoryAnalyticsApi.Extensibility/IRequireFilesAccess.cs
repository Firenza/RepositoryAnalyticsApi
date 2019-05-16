using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireFilesAccess
    {
        IEnumerable<RepositoryFile> Files { get; set; }
    }
}
