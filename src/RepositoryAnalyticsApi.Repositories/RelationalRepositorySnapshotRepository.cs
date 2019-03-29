using Microsoft.Extensions.Logging;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class RelationalRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private string mySqlConnectionString;
        private ILogger<RelationalRepositorySnapshotRepository> logger;
        private IVersionManager versionManager;

        public RelationalRepositorySnapshotRepository(string mySqlConnectionString, ILogger<RelationalRepositorySnapshotRepository> logger, IVersionManager versionManager)
        {
            this.mySqlConnectionString = mySqlConnectionString;
            this.logger = logger;
            this.versionManager = versionManager;
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            throw new NotImplementedException();
        }

        public Task<RepositorySnapshot> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        private async Task DeleteChildrenAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task UpsertAsync(RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            throw new NotImplementedException();
        }
    }
}

