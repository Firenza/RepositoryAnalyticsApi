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
    public class RelationalDependencyRepository : IDependencyRepository
    {
        private RepositoryAnalysisContext repositoryAnalysisContext;
        private IMapper mapper;

        public RelationalDependencyRepository(RepositoryAnalysisContext repositoryAnalysisContext, IMapper mapper)
        {
            this.repositoryAnalysisContext = repositoryAnalysisContext;
            this.mapper = mapper;
        }

        public Task<List<RepositoryDependencySearchResult>> SearchAsync(string name, RepositorySearch repositorySearch)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> SearchNamesAsync(string name, DateTime? asOf)
        {
            var dbConnection = repositoryAnalysisContext.Database.GetDbConnection();

            var matchingNames = await dbConnection.QueryAsync<string>(
                 @"SELECT name
                      FROM repository_dependency
                      WHERE name LIKE @Name
                      GROUP BY name
                      ORDER BY name",
                 new { Name = "%" + name + "%" });

            if (matchingNames != null)
            {
                return matchingNames.ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
