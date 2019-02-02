using MongoDB.Bson;
using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var snapshotFilterArray = new BsonArray();
            var repositorySnapshotFilters = MongoFilterFactory.RepositorySnapshotFilters(new RepositorySearch {AsOf = asOf });
            snapshotFilterArray.AddRange(repositorySnapshotFilters);

            var filter = new BsonDocument("$and", snapshotFilterArray);

            var distinctFieldName = $"{nameof(RepositorySnapshot.TypesAndImplementations)}.{nameof(RepositoryTypeAndImplementations.TypeName)}";

            using (var cursor = await mongoCollection.DistinctAsync<string>(distinctFieldName, filter).ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync().ConfigureAwait(false))
                {
                    var batch = cursor.Current;
                    foreach (var typeName in batch)
                    {
                        typeNames.Add(typeName);
                    }
                }
            }

            // Sort these after the query as there shouldn't be too many AND I'm not sure how to do sorting
            // with the "DistinctAsync" call on the client
            var orderedTypeNames = typeNames.OrderBy(typeName => typeName).ToList();

            return orderedTypeNames;
        }
    }
}