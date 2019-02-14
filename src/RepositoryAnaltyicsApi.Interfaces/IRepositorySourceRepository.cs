using RepositoryAnalyticsApi.InternalModel;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceRepository
    {
        Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string gitRef);
        Task<RepositorySourceRepository> ReadRepositoryAsync(string repositoryOwner, string repositoryName);
        Task<string> ReadFileContentAsync(string owner, string name, string fullFilePath, string gitRef);
        Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string gitRef, List<string> fullFilePaths);
        Task<CursorPagedResults<RepositorySummary>> ReadRepositorySummariesAsync(string organization, string user, int take, string endCursor);
        Task<RepositorySummary> ReadRepositorySummaryAsync(string organization, string user, string name);
        Task<RepositorySourceSnapshot> ReadRepositorySourceSnapshotAsync(string organization, string user, string name, string branch, DateTime? asOf);
        Task<Dictionary<string, List<TeamRepositoryConnection>>> ReadTeamToRepositoriesMaps(string organization);
        Task<OwnerType> ReadOwnerType(string owner);
    }
}
