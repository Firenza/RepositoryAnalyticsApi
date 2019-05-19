using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireTopicsAccess
    {
        IEnumerable<string> TopicNames { get; set; }
    }
}
