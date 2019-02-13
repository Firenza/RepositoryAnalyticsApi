using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private string mySqlConnectionString;
        private ILogger<MySqlRepositorySnapshotRepository> logger;
        private IVersionManager versionManager;

        public MySqlRepositorySnapshotRepository(string mySqlConnectionString, ILogger<MySqlRepositorySnapshotRepository> logger, IVersionManager versionManager)
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
            using (var mySqlConnection = new MySqlConnection(mySqlConnectionString))
            using (Operation.Time("Snapshot Children Deletion"))
            {
                await mySqlConnection.OpenAsync();

                var trans = await mySqlConnection.BeginTransactionAsync().ConfigureAwait(false);

                // For now just always delete all existing child tables
                await mySqlConnection.ExecuteAsync(
                     @"DELETE 
                        FROM RepositoryDependencies
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                     new { RepositorySnapshotId = id },
                     trans).ConfigureAwait(false);

                await mySqlConnection.ExecuteAsync(
                    @"DELETE 
                        FROM RepositoryFiles
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                    new { RepositorySnapshotId = id },
                    trans).ConfigureAwait(false);

                // Foreign key constraint will cascade delete any child implementations
                await mySqlConnection.ExecuteAsync(
                   @"DELETE 
                        FROM RepositoryTypes
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                   new { RepositorySnapshotId = id },
                   trans).ConfigureAwait(false);

                trans.Commit();
            }
        }


        public async Task UpsertAsync(RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            var mappedRepositorySnapshot = Model.MySql.RepositorySnapshot.MapFrom(snapshot, repositoryCurrentStateId.Value);

            using (var mySqlConnection = new MySqlConnection(mySqlConnectionString))
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                int existingRecordId = 0;

                using (Operation.Time("Snapshot Find"))
                {
                    existingRecordId = await mySqlConnection.ExecuteScalarAsync<int>(
                         @"SELECT Id 
                      FROM RepositorySnapshots
                      WHERE WindowStartCommitId = @WindowStartCommitId",
                         new { WindowStartCommitId = snapshot.WindowStartCommitId }).ConfigureAwait(false);
                }

                if (existingRecordId == 0)
                {
                    using (Operation.Time("Snapshot Insert"))
                    {
                        existingRecordId = await mySqlConnection.InsertAsync(mappedRepositorySnapshot).ConfigureAwait(false);
                    }
                }
                else
                {
                    mappedRepositorySnapshot.Id = existingRecordId;

                    using (Operation.Time("Snapshot Update"))
                    {
                        await mySqlConnection.UpdateAsync(mappedRepositorySnapshot).ConfigureAwait(false);
                    }

                    await DeleteChildrenAsync(existingRecordId);
                }

                var mappedFiles = Model.MySql.RepositoryFile.MapFrom(snapshot, existingRecordId);

                using (Operation.Time($"Snapshot {mappedFiles.Count} Files Insert"))
                {
                    var trans = await mySqlConnection.BeginTransactionAsync();
                    await mySqlConnection.InsertAsync(mappedFiles, trans).ConfigureAwait(false);
                    trans.Commit();
                }

                var mappedDependencies = Model.MySql.RepositoryDependency.MapFrom(snapshot, existingRecordId, versionManager);

                using (Operation.Time($"Snapshot {mappedDependencies.Count} Dependencies Insert"))
                {
                    var trans = await mySqlConnection.BeginTransactionAsync();
                    await mySqlConnection.InsertAsync(mappedDependencies, trans).ConfigureAwait(false);
                    trans.Commit();
                }

                var mappedTypes = Model.MySql.RepositoryType.MapFrom(snapshot, existingRecordId);

                using (Operation.Time("Snapshot Types and Implementations Insert"))
                {
                    foreach (var mappedType in mappedTypes)
                    {
                        var typeId = await mySqlConnection.InsertAsync(mappedType).ConfigureAwait(false);

                        // Get matching implementation list
                        var matchingTypeAndImplementation = snapshot.TypesAndImplementations.FirstOrDefault(typeAndImplementation => typeAndImplementation.TypeName == mappedType.Name);

                        var mappedImplementations = Model.MySql.RepositoryImplementation.MapFrom(matchingTypeAndImplementation.Implementations, typeId);

                        await mySqlConnection.InsertAsync(mappedImplementations).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}

