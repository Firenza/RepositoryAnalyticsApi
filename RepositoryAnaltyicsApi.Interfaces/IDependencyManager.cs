using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyManager
    {
        List<RepositoryDependency> Read(string repositoryId);
        
        /// <summary>
        /// A regular expression identifying which repository files need to find the depencencies.  Used for bulk file content requests.
        /// </summary>
        Regex SourceFileRegex {get; }
    }
}
