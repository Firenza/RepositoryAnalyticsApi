using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories
{
    public class GitHubV3ApiRepositorySourceRepository : IRepositorySourceRepository
    {
        public string GetFileContent(string repositoryId, string fullFilePath)
        {
            throw new NotImplementedException();
        }

        public List<RepositoryFile> ReadFiles(string repositoryId, string branch)
        {
            throw new NotImplementedException();
        }

        public List<Repository> ReadRepositories(string group, int pageCount, int pageSize, int startPage)
        {
            throw new NotImplementedException();
        }

        public Repository ReadRepository(string repositoryId)
        {
            throw new NotImplementedException();
        }
    }
}
