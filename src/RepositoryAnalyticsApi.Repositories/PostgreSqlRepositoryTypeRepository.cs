using Dapper;
using Microsoft.EntityFrameworkCore;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class PostgreSqlRepositoryTypeRepository : IRepositoryTypeRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;

        public PostgreSqlRepositoryTypeRepository(RepositoryAnalysisContext repositoryAnalysisContext)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
        }

        public async Task<List<CountAggregationResult>> ReadAllAsync()
        {
            var matchingDependencyNames = new List<string>();

            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var countAggregationResults = await dbConnection.QueryAsync<CountAggregationResult>(
                    @"select RTI.type_name as name, Count(*)
                    FROM repository_implementation RI
                    join repository_type_and_implementations RTI
	                    on RTI.repository_type_and_implementations_id = RI.repository_type_and_implementations_id
                    join repository_snapshot RS
	                    on RS.repository_snapshot_id = RTI.repository_snapshot_id
                            -- This will cause only the current state of the repositories to be included in the results
	                    and RS.window_ends_on is null
                    join repository_current_state RCS
	                    on RCS.repository_current_state_id = RS.repository_current_state_id
                    group by RTI.type_name
                    order by count desc");

            return countAggregationResults.AsList();
        }
    }
}
