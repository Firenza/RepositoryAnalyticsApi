using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class RelationalRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private ILogger<RelationalRepositorySnapshotRepository> logger;
        private IVersionManager versionManager;
        private IMapper mapper;

        public RelationalRepositorySnapshotRepository(
            RepositoryAnalysisContext repositoryAnalysisContext,
            ILogger<RelationalRepositorySnapshotRepository> logger,
            IVersionManager versionManager,
            IMapper mapper)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.logger = logger;
            this.versionManager = versionManager;
            this.mapper = mapper;
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ServiceModel.RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            var repositorySnapshots = new List<ServiceModel.RepositorySnapshot>();

            var dbRepositoryCurrentState = await repositoryAnalysisContext
                                          .RepositoryCurrentState
                                          .AsNoTracking()
                                          .Include(rcs => rcs.RepositorySnapshots)
                                          .Where(rcs => rcs.RepositoryId == repositoryCurrentStateId)
                                          .SingleOrDefaultAsync();

            if (dbRepositoryCurrentState?.RepositorySnapshots != null)
            {
                repositorySnapshots = mapper.Map<List<ServiceModel.RepositorySnapshot>>(dbRepositoryCurrentState.RepositorySnapshots);
            }

            return repositorySnapshots;
        }

        public Task<ServiceModel.RepositorySnapshot> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task UpsertAsync(ServiceModel.RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            var dbRepositoryCurrentState = await repositoryAnalysisContext
                                          .RepositoryCurrentState
                                          .Include(rcs => rcs.RepositorySnapshots)
                                            .ThenInclude(rs => rs.Dependencies)
                                          .Include(rcs => rcs.RepositorySnapshots)
                                            .ThenInclude(rs => rs.Files)
                                          .Include(rcs => rcs.RepositorySnapshots)
                                            .ThenInclude(rs => rs.TypesAndImplementations)
                                                .ThenInclude(rti => rti.Implementations)
                                          .Where(rcs => rcs.RepositorySnapshots.Any(rs => rs.WindowStartCommitId == snapshot.WindowStartCommitId))
                                          .SingleOrDefaultAsync();

            if (dbRepositoryCurrentState == null)
            {
                if (!repositoryCurrentStateId.HasValue || repositoryCurrentStateId.Value == 0)
                {
                    throw new ArgumentException("Unable to insert new Repository Snapshot without the Current State Id");
                }

                var dbSnapshot = mapper.Map<RepositorySnapshot>(snapshot);
                dbSnapshot.RepositoryCurrentStateId = repositoryCurrentStateId.Value;

                repositoryAnalysisContext.Add(dbSnapshot);
            }
            else
            {
                var dbSnapshot = dbRepositoryCurrentState.RepositorySnapshots.First();

                //If the object already exists in the DB, then map the model object into this
                // already existing db objec to take advantage of EF update tracking
                mapper.Map(snapshot, dbSnapshot);
            }

            await repositoryAnalysisContext.SaveChangesAsync();
        }
    }
}

