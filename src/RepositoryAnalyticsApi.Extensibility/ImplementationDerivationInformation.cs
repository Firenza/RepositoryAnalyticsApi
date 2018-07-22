using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    /// <summary>
    /// Additional repository infomration and helper methods to assist in the derivation process
    /// </summary>
    public class ImplementationDerivationInformation
    {
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }
        public IEnumerable<string> Topics { get; set; }
        public Func<string, Task<string>> ReadFileContentAsync { get; set; }
        public Func<Task<List<RepositoryFile>>> ReadFilesAsync { get; set; }
    }
}
