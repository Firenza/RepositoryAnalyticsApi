using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IDeriveRepositoryTypeAndImplementations
    {
        /// <summary>
        /// Derives repository type and implementation information from provided repository information /s methods
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="files"></param>
        /// <param name="topics"></param>
        /// <param name="name"></param>
        /// <param name="readFileContentAsync"></param>
        /// <returns></returns>
        Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync);
    }
}
