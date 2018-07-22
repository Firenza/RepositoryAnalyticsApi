using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireFileListAccess
    {
        Func<Task<List<RepositoryFile>>> ReadFileListAsync { get; set; }
    }
}
