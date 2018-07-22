using MongoDB.Bson;
using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoriesTypeNamesRepository : IRepositoriesTypeNamesRepository
    {
        private IMongoCollection<RepositorySnapshot> mongoCollection;

        public MongoRepositoriesTypeNamesRepository(IMongoCollection<RepositorySnapshot> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task<List<string>> ReadAsync(DateTime? asOf)
        {
            var typeNames = new List<string>();

            FilterDefinition<RepositorySnapshot> filter = null;

            if (asOf.HasValue)
            {
                filter = Builders<RepositorySnapshot>.Filter.And(
                    Builders<RepositorySnapshot>.Filter.Gte(snapshot => snapshot.WindowStartsOn, new BsonDateTime(asOf.Value)),
                    Builders<RepositorySnapshot>.Filter.Lte(snapshot => snapshot.WindowEndsOn, new BsonDateTime(asOf.Value))
                    );
            }
            else
            {
                filter = Builders<RepositorySnapshot>.Filter.Eq(snapshot => snapshot.WindowEndsOn, null);
            }

            using (var cursor = await mongoCollection.DistinctAsync<string>("TypesAndImplementations.TypeName", filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var typeName in batch)
                    {
                        typeNames.Add(typeName);
                    }
                }
            }

            return typeNames;
        }
    }
}