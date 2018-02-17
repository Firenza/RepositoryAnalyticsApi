using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryRepository : IRepositoryRepository
    {
        private IMongoCollection<Repository> mongoCollection;

        public MongoRepositoryRepository(IMongoCollection<Repository> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task CreateAsync(Repository repository)
        {
            await mongoCollection.InsertOneAsync(repository);
        }

        public async Task DeleteAsync(string id)
        {
            await mongoCollection.DeleteOneAsync(repostiory => repostiory.Id == id);
        }

        public async Task<Repository> ReadAsync(string id)
        {
            var cursor = await mongoCollection.FindAsync(reposity => reposity.Id == id);
            var repository = await cursor.FirstOrDefaultAsync();

            return repository;
        }

        public async Task UpdateAsync(Repository repository)
        {
            await mongoCollection.ReplaceOneAsync(repo => repo.Id == repository.Id, repository);
            
        }
    }
}