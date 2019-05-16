using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireBacklogInfoAccess
    {
        BacklogInfo BacklogInfo { get; set; }
    }
}
