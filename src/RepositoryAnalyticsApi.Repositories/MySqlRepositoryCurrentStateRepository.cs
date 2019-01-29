using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using Serilog;
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
                await mySqlConnection.OpenAsync();

                var timer = Stopwatch.StartNew();

                var existingRecordId = await mySqlConnection.ExecuteScalarAsync<int>(
                    @"SELECT Id 
                      FROM RepositoryCurrentStates
                      WHERE Name = @RepositoryName",
                    new { RepositoryName = repositoryCurrentState.Name });

                if (existingRecordId == 0)
                {
                    existingRecordId = await mySqlConnection.InsertAsync(mappedRepositoryCurrentState);
                }
                else
                {
                    await mySqlConnection.UpdateAsync(mappedRepositoryCurrentState);

                    // For now just always delete all existing child tables
                    await mySqlConnection.ExecuteAsync(
                         @"DELETE 
                        FROM Teams
                        WHERE RepositoryCurrentStateId = @RepositoryCurrentStateId",
                         new { RepositoryCurrentStateId = existingRecordId });

                    await mySqlConnection.ExecuteAsync(
                        @"DELETE 
                        FROM Topics
                        WHERE RepositoryCurrentStateId = @RepositoryCurrentStateId",
                        new { RepositoryCurrentStateId = existingRecordId });
                }

                var mappedTeams = Model.MySql.Team.MapFrom(repositoryCurrentState, existingRecordId);

                await mySqlConnection.InsertAsync(mappedTeams);

                var mappedTopics = Model.MySql.Topic.MapFrom(repositoryCurrentState, existingRecordId);

                await mySqlConnection.InsertAsync(mappedTopics);

                timer.Stop();

                Log.Logger.Debug($"Upsert of {nameof(RepositoryCurrentState)} took {timer.ElapsedMilliseconds} milliseconds");

                return existingRecordId;
            }
        }
    }
}