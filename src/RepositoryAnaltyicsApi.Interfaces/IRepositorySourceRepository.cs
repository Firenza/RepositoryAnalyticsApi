using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceRepository
    {
        Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch);
        Task<Repository> ReadRepositoryAsync(string repositoryOwner, string repositoryName);
        Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath);
        Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths);
        Task<CursorPagedResults<RepositorySummary>> ReadRepositorySummariesAsync(string organization, string user, int take, string endCursor, DateTime? asOf);
        Task<Dictionary<string, List<string>>> ReadTeamToRepositoriesMaps(string organization);
    }
}
