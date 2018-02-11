using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceManager
    {
        Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch);
        Task<Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName);
        Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath);
        List<(string fullFilePath, string fileContent)> GetMultipleFileContents(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths);
    }
}
