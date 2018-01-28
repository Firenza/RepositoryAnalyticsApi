using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositorySourceRepository
    {
        List<RepositoryFile> ReadFiles(string repositoryId, string branch);
        List<Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage);
        Repository ReadRepository(string repositoryId);
        string GetFileContent(string repositoryId, string fullFilePath);
    }
}
