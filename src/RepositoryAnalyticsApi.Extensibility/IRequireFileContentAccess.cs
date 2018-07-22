using System;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireFileContentAccess
    {
        /// <summary>
        /// Takes in a full file path and returns the content
        /// </summary>
        Func<string, Task<string>> ReadFileContentAsync { get; set; }
    }
}
