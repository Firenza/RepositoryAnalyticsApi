using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibliity
{
    public interface IDeriveRepositoryTypeAndImplementations
    {
       /// <summary>
       /// The type of repository derived (E.G. Web UI, Console Application, API)
       /// </summary>
       string TypeName { get;  }

        /// <summary>
        /// The implentaiton name of repository derived (E.G. React, Angular, Node JS, ASP.NET Core)
        /// </summary>
       string ImplementationName { get;  }

        /// <summary>
        /// Derives repository type and implementation information from provided repository information /s methods
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="files"></param>
        /// <param name="topics"></param>
        /// <param name="name"></param>
        /// <param name="readFileContentAsync"></param>
        /// <returns></returns>
        Task<RespositoryTypeAndImplementationInfo> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync);
    }
}
