using Microsoft.Extensions.Logging;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class RelationalRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private ILogger<RelationalRepositorySnapshotRepository> logger;
        private IVersionManager versionManager;

        public RelationalRepositorySnapshotRepository(RepositoryAnalysisContext repositoryAnalysisContext, ILogger<RelationalRepositorySnapshotRepository> logger, IVersionManager versionManager)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.logger = logger;
            this.versionManager = versionManager;
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<ServiceModel.RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceModel.RepositorySnapshot> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        private async Task DeleteChildrenAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task UpsertAsync(ServiceModel.RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            throw new NotImplementedException();
        }
    }
}

