using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using Serilog;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositoryCurrentStateRepository : IRepositoryCurrentStateRepository
    {
        private string mySqlConnectionString;

        public MySqlRepositoryCurrentStateRepository(string mySqlConnectionString)
        {
            this.mySqlConnectionString = mySqlConnectionString;
        }

        public async Task<int?> UpsertAsync(RepositoryCurrentState repositoryCurrentState)
        {
            var mappedRepositoryCurrentState = Model.MySql.RepositoryCurrentState.MapFrom(repositoryCurrentState);

            using (var mySqlConnection = new MySqlConnection(mySqlConnectionString))
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                int existingRecordId = 0;

                using (Operation.Time("Current State Find"))
                {
                    existingRecordId = await mySqlConnection.ExecuteScalarAsync<int>(
                     @"SELECT Id 
                      FROM RepositoryCurrentStates
                      WHERE Name = @RepositoryName",
                                        new { RepositoryName = repositoryCurrentState.Name }).ConfigureAwait(false);
                }

                if (existingRecordId == 0)
                {
                    using (Operation.Time("Current State Insert"))
                    {
                        existingRecordId = await mySqlConnection.InsertAsync(mappedRepositoryCurrentState).ConfigureAwait(false);
                    }
                }
                else
                {
                    using (Operation.Time("Current State Update"))
                    {
                        await mySqlConnection.UpdateAsync(mappedRepositoryCurrentState).ConfigureAwait(false);
                    }

                    using (Operation.Time("Current State Children Delete"))
                    {
                        // For now just always delete all existing child tables
                        await mySqlConnection.ExecuteAsync(
                             @"DELETE 
                        FROM Teams
                        WHERE RepositoryCurrentStateId = @RepositoryCurrentStateId",
                             new { RepositoryCurrentStateId = existingRecordId }).ConfigureAwait(false);

                        await mySqlConnection.ExecuteAsync(
                            @"DELETE 
                        FROM Topics
                        WHERE RepositoryCurrentStateId = @RepositoryCurrentStateId",
                            new { RepositoryCurrentStateId = existingRecordId }).ConfigureAwait(false);
                    }
                }

                var mappedTeams = Model.MySql.Team.MapFrom(repositoryCurrentState, existingRecordId);

                using (Operation.Time("Current State Teams Insert"))
                {
                    await mySqlConnection.InsertAsync(mappedTeams).ConfigureAwait(false);
                }

                var mappedTopics = Model.MySql.Topic.MapFrom(repositoryCurrentState, existingRecordId);

                using (Operation.Time("Current State Topics Insert"))
                {
                    await mySqlConnection.InsertAsync(mappedTopics).ConfigureAwait(false);
                }

                return existingRecordId;
            }
        }
    }
}