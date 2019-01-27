using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryCurrentStateRepository : IRepositoryCurrentStateRepository
    {
        private IMongoCollection<RepositoryCurrentState> mongoCollection;

        public MongoRepositoryCurrentStateRepository(IMongoCollection<RepositoryCurrentState> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }
        public async Task<int?> UpsertAsync(RepositoryCurrentState repository)
        {
            var filter = Builders<RepositoryCurrentState>.Filter.Eq(repo => repo.Id, repository.Id);

            await mongoCollection.ReplaceOneAsync(filter, repository, new UpdateOptions { IsUpsert = true});

            return null;
        }
    }
}
