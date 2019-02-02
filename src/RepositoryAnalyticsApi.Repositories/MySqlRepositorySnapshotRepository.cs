using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerilogTimings;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private string mySqlConnectionString;
        private ILogger<MySqlRepositorySnapshotRepository> logger;

        public MySqlRepositorySnapshotRepository(string mySqlConnectionString, ILogger<MySqlRepositorySnapshotRepository> logger)
        {
            this.mySqlConnectionString = mySqlConnectionString;
            this.logger = logger;
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

        public async Task UpsertAsync(RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            var mappedRepositorySnapshot = Model.MySql.RepositorySnapshot.MapFrom(snapshot, repositoryCurrentStateId.Value);

            using (var mySqlConnection = new MySqlConnection(mySqlConnectionString))
            {
                await mySqlConnection.OpenAsync();

                int existingRecordId = 0;

                using (Operation.Time("Snapshot Find"))
                {
                    existingRecordId = await mySqlConnection.ExecuteScalarAsync<int>(
                         @"SELECT Id 
                      FROM RepositorySnapshots
                      WHERE WindowStartCommitId = @WindowStartCommitId",
                         new { WindowStartCommitId = snapshot.WindowStartCommitId });
                }

                if (existingRecordId == 0)
                {
                    using (Operation.Time("Snapshot Insert"))
                    {
                        existingRecordId = await mySqlConnection.InsertAsync(mappedRepositorySnapshot);
                    }
                }
                else
                {
                    mappedRepositorySnapshot.Id = existingRecordId;

                    using (Operation.Time("Snapshot Update"))
                    {
                        await mySqlConnection.UpdateAsync(mappedRepositorySnapshot);
                    }

                    using (Operation.Time("Snapshot Children Deletion"))
                    {
                        var trans = await mySqlConnection.BeginTransactionAsync();

                        // For now just always delete all existing child tables
                        await mySqlConnection.ExecuteAsync(
                             @"DELETE 
                        FROM RepositoryDependencies
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                             new { RepositorySnapshotId = existingRecordId },
                             trans);

                        await mySqlConnection.ExecuteAsync(
                            @"DELETE 
                        FROM RepositoryFiles
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                            new { RepositorySnapshotId = existingRecordId },
                            trans);

                        // Foreign key constraint will cascade delete any child implementations
                        await mySqlConnection.ExecuteAsync(
                           @"DELETE 
                        FROM RepositoryTypes
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                           new { RepositorySnapshotId = existingRecordId },
                           trans);

                        trans.Commit();
                    }
                }

                var mappedFiles = Model.MySql.RepositoryFile.MapFrom(snapshot, existingRecordId);

                using (Operation.Time($"Snapshot {mappedFiles.Count} Files Insert"))
                {
                    var trans = await mySqlConnection.BeginTransactionAsync();
                    await mySqlConnection.InsertAsync(mappedFiles, trans);
                    trans.Commit();

                }

                var mappedDependencies = Model.MySql.RepositoryDependency.MapFrom(snapshot, existingRecordId);

                using (Operation.Time($"Snapshot {mappedDependencies.Count} Dependencies Insert"))
                {
                    var trans = await mySqlConnection.BeginTransactionAsync();
                    await mySqlConnection.InsertAsync(mappedDependencies, trans);
                    trans.Commit();
                }

                var mappedTypes = Model.MySql.RepositoryType.MapFrom(snapshot, existingRecordId);

                using (Operation.Time("Snapshot Types and Implementations Insert"))
                {
                    foreach (var mappedType in mappedTypes)
                    {
                        var typeId = await mySqlConnection.InsertAsync(mappedType);

                        // Get matching implementation list
                        var matchingTypeAndImplementation = snapshot.TypesAndImplementations.FirstOrDefault(typeAndImplementation => typeAndImplementation.TypeName == mappedType.Name);

                        var mappedImplementations = Model.MySql.RepositoryImplementation.MapFrom(matchingTypeAndImplementation.Implementations, typeId);

                        await mySqlConnection.InsertAsync(mappedImplementations);
                    }
                }
            }
        }
    }
}

