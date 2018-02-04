using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyManager
    {
        Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch);
        
        /// <summary>
        /// A regular expression identifying which repository files need to find the depencencies.  Used for bulk file content requests.
        /// </summary>
        Regex SourceFileRegex {get; }
    }
}
