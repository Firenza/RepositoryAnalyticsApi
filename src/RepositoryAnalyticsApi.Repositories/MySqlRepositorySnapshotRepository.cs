using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private MySqlConnection mySqlConnection;

        public MySqlRepositorySnapshotRepository(MySqlConnection mySqlConnection)
        {
            this.mySqlConnection = mySqlConnection;
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

            var existingRecordId = await mySqlConnection.ExecuteScalarAsync<int>(
                     @"SELECT Id 
                      FROM RepositorySnapshots
                      WHERE WindowStartCommitId = @WindowStartCommitId",
                     new { WindowStartCommitId = snapshot.WindowStartCommitId });

            if (existingRecordId == 0)
            {
                existingRecordId = await mySqlConnection.InsertAsync(mappedRepositorySnapshot);
            }
            else
            {
                mappedRepositorySnapshot.Id = existingRecordId;

                await mySqlConnection.UpdateAsync(mappedRepositorySnapshot);

                // For now just always delete all existing child tables
                await mySqlConnection.ExecuteAsync(
                     @"DELETE 
                        FROM RepositoryDependencies
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                     new { RepositorySnapshotId = existingRecordId });

                await mySqlConnection.ExecuteAsync(
                    @"DELETE 
                        FROM RepositoryFiles
                        WHERE RepositorySnapshotId = @RepositorySnapshotId",
                    new { RepositorySnapshotId = existingRecordId });
            }

            var mappedFiles = Model.MySql.RepositoryFile.MapFrom(snapshot, existingRecordId);

            await mySqlConnection.InsertAsync(mappedFiles);

            var mappedDependencies = Model.MySql.RepositoryDependency.MapFrom(snapshot, existingRecordId);

            await mySqlConnection.InsertAsync(mappedDependencies);
        }
    }
}
