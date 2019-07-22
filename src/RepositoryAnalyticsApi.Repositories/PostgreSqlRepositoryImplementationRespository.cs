using Dapper;
using Microsoft.EntityFrameworkCore;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class PostgreSqlRepositoryImplementationRespository : IRepositoryImplementationsRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;

        public PostgreSqlRepositoryImplementationRespository(RepositoryAnalysisContext repositoryAnalysisContext)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
        }

        public async Task<List<CountAggregationResult>> SearchAsync(RepositorySearch repositorySearch)
        {
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var countAggregationResults = await dbConnection.QueryAsync<CountAggregationResult>(
                    @"select RI.name, Count(*)
                    FROM repository_implementation RI
                    join repository_type_and_implementations RTI
                        on RTI.repository_type_and_implementations_id = RI.repository_type_and_implementations_id
                    join repository_snapshot RS
                        on RS.repository_snapshot_id = RTI.repository_snapshot_id
                            -- This will cause only the current state of the repositories to be included in the results
                        and RS.window_ends_on is null
                    where RTI.type_name = @RepositoryTypeName
                    group by RI.name
                    order by count desc",
                    new {RepositoryTypeName = repositorySearch.TypeName });

            return countAggregationResults.AsList();
        }
    }
}
