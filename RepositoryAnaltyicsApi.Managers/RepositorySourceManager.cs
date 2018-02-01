using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositorySourceManager : IRepositorySourceManager
    {
        private IRepositorySourceRepository repositorySourceRepository;

        public RepositorySourceManager(IRepositorySourceRepository repositorySourceRepository)
        {
            this.repositorySourceRepository = repositorySourceRepository;
        }

        public string ReadFileContent(string repositoryId, string fullFilePath)
        {
            throw new NotImplementedException();
        }

        public List<RepositoryFile> ReadFiles(string repositoryId)
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
