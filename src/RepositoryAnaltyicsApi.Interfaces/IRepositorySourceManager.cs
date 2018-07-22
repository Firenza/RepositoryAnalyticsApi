using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceManager
    {
        Task<List<RepositoryFile>> ReadFilesAsync(string owner, string name, string branch, DateTime? asOf = null);
        Task<RepositorySourceRepository> ReadRepositoryAsync(string repositoryOwner, string repositoryName);
        Task<string> ReadFileContentAsync(string owner, string name, string branch, string fullFilePath, DateTime? asOf = null);
        Task<List<(string fullFilePath, string fileContent)>> GetMultipleFileContentsAsync(string repositoryOwner, string repositoryName, string branch, List<string> fullFilePaths, DateTime? asOf = null);
        Task<RepositorySummary> ReadRepositorySummaryAsync(string owner, string name, string branch, DateTime? asOf);
        Task<CursorPagedResults<RepositorySummary>> ReadRepositoriesAsync(string owner, int take, string endCursor, DateTime? asOf);
        Task<RepositorySourceSnapshot> ReadRepositorySourceSnapshotAsync(string owner, string name, string branch, DateTime? asOf);
        Task<OwnerType> ReadOwnerType(string owner);
    }
}
