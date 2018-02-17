using System.Diagnostics;

namespace RepositoryAnalyticsApi.ServiceModel
{
    [DebuggerDisplay("Name = {Name}, Verion = {Version}, Source = {Source}")]
    public class RepositoryDependency
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string MajorVersion { get; set; }
        /// <summary>
        /// Anything in the version string after a dash.  Stored here as the Version class doesn't know how to handle it
        /// </summary>
        public string PreReleaseSemanticVersion { get; set; }
        /// <summary>
        /// Where the dependency is needed. Mainly applies to NPM / Bower dependencies although NuGet does have this feature.
        /// </summary>
        public string Environment { get; set; }
        /// <summary>
        /// NPM, Bower, NuGet, Visual Studio Project File
        /// </summary>
        public string Source { get; set; }
        public string RepoPath { get; set; }
    }
}
