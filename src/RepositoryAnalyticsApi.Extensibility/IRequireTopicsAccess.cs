using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireTopicsAccess
    {
        IEnumerable<string> TopicNames { get; set; }
    }
}
