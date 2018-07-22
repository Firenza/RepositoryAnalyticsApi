using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyScraperManager
    {
        Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch, DateTime? asOf);
        
        /// <summary>
        /// A regular expression identifying which repository files need to find the depencencies.  Used for bulk file content requests.
        /// </summary>
        Regex SourceFileRegex {get; }
    }
}
