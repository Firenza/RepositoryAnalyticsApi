using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceManager
    {
        List<RepositoryFile> ReadFiles(string repositoryId);
        List<Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage);
        Repository ReadRepository(string repositoryId);
        string GetFileContent(string repositoryId, string fullFilePath);
    }
}
