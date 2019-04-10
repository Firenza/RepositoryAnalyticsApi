using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class Aggregations
    {
        public int Count { get; set; }
    }

    public class RelationalDependencyRepository : IDependencyRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private IMapper mapper;

        public RelationalDependencyRepository(RepositoryAnalysisContext repositoryAnalysisContext, IMapper mapper)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.mapper = mapper;
        }

        public async Task<List<RepositoryDependencySearchResult>> ReadAsync(string name, RepositorySearch repositorySearch)
        {
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var respositoryDependencySearchResults = await dbConnection.QueryAsync<Model.EntityFramework.RepositoryDependency, RepositoryDependencySearchResult, RepositoryDependencySearchResult>(
                @"SELECT name, version, Count(*) count
                FROM repository_dependency  RD
                JOIN repository_snapshot RS 
	                on RS.repository_snapshot_id = RD.repository_snapshot_id
	                and RS.window_ends_on is null
                WHERE name = @name
                GROUP BY name, version, padded_version
                order by padded_version",
                (repositoryDepenency, repositoryDependencySearchResult) => 
                {
                    repositoryDependencySearchResult.RepositoryDependency = mapper.Map<ServiceModel.RepositoryDependency>(repositoryDepenency);

                    return repositoryDependencySearchResult;
                },
                new { name = name },
                splitOn: "count");

            if (respositoryDependencySearchResults != null && respositoryDependencySearchResults.Any())
            {
                return respositoryDependencySearchResults.ToList();
            }
            else
            {
                return new List<RepositoryDependencySearchResult>();
            }
        }

        public async Task<List<string>> SearchNamesAsync(string name, DateTime? asOf)
        {
            var matchingDependencyNames = new List<string>();

            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            if (!asOf.HasValue)
            {
                var startsWithMatchesTask = dbConnection.QueryAsync<string>(
                         @"SELECT name
                            FROM repository_dependency  RD
                            JOIN repository_snapshot RS 
	                            on RS.repository_snapshot_id = RD.repository_snapshot_id
	                            and RS.window_ends_on is null
                            WHERE name LIKE @Name
                            GROUP BY name
                            ORDER BY name",
                         new { Name = name + "%" });

                var startsWithMatches = await startsWithMatchesTask;

                var anyMatchesTask = dbConnection.QueryAsync<string>(
                     @"SELECT name
                        FROM repository_dependency RD
                        JOIN repository_snapshot RS 
	                        on RS.repository_snapshot_id = RD.repository_snapshot_id
	                        and RS.window_ends_on is null
                        WHERE name LIKE @Name
                        GROUP BY name
                        ORDER BY name",
                     new { Name = "%" + name + "%" });

                var anyMatches = await anyMatchesTask;

                if (startsWithMatches.Any())
                {
                    matchingDependencyNames.AddRange(startsWithMatches);
                }
                else if (matchingDependencyNames.Count < 10 && anyMatches.Any())
                {
                    matchingDependencyNames.AddRange(anyMatches);
                }
            }

            return matchingDependencyNames;
        }
    }
}
