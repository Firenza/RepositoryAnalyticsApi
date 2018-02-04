using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoDependencyRepository : IDependencyRepository
    {
        private IMongoCollection<Repository> mongoCollection;

        public MongoDependencyRepository(IMongoCollection<Repository> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task<List<RepositoryDependency>> SearchAsync(string name)
        {
            // var cursor = await mongoCollection.FindAsync(reposity => reposity.Id == id);
            //var repository = await cursor.FirstOrDefaultAsync();

            //return repository;
            return null;
        }
    }
}
