using System.Diagnostics;

namespace RepositoryAnalyticsApi.ServiceModel
{
    [DebuggerDisplay("Name = {Name}, FullPath = {FullPath}")]
    public class RepositoryFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
    }
}
