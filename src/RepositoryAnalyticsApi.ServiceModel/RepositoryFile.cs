using System;
using System.Diagnostics;

namespace RepositoryAnalyticsApi.ServiceModel
{
    [DebuggerDisplay("Name = {Name}, FullPath = {FullPath}")]
    [Serializable]
    public class RepositoryFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
    }
}
