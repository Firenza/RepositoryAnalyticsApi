using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceRepository
    {
        List<RepositoryFile> ReadFiles(string owner, string name, string branch);
        List<Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage);
        Task<Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName);
        Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath);
        List<(string fullFilePath, string fileContent)> GetMultipleFileContents(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths);
    }
}
