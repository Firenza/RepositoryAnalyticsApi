using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositorySearchRepository : IRepositorySearchRepository
    {
        private IMongoCollection<BsonDocument> mongoCollection;

        public MongoRepositorySearchRepository(IMongoCollection<BsonDocument> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task<List<string>> SearchAsync(RepositorySearch repositorySearch)
        {
            var foundRepositoryNames = new List<string>();

            var preLookupSnapshotFilterArray = new BsonArray();
            var repositorySnapshotFilters = MongoFilterFactory.RepositorySnapshotFilters(repositorySearch);
            preLookupSnapshotFilterArray.AddRange(repositorySnapshotFilters);

            var postLookupCurrentStateFilterArray = new BsonArray();
            var repositoryCurrenStateFilters = MongoFilterFactory.RepositoryCurrenStatePostLookupFilters(repositorySearch);
            postLookupCurrentStateFilterArray.AddRange(repositoryCurrenStateFilters);

            var pipelineBsonDocuments = new List<BsonDocument>();

            pipelineBsonDocuments.AddRange(new List<BsonDocument> {
                // Can always add this since there will always be a window filter query
                new BsonDocument("$match", new BsonDocument()
                    .Add("$and", preLookupSnapshotFilterArray)),
                new BsonDocument("$lookup", new BsonDocument()
                    .Add("from", "repositoryCurrentState")
                    .Add("localField", "RepositoryCurrentStateId")
                    .Add("foreignField", "_id")
                    .Add("as", "RepositoryCurrentState"))
            });

            if (postLookupCurrentStateFilterArray.Count > 0)
            {
                pipelineBsonDocuments.Add(new BsonDocument("$match", new BsonDocument()
                    .Add("$and", postLookupCurrentStateFilterArray))
                );
            }

            pipelineBsonDocuments.Add(
                new BsonDocument("$project", new BsonDocument()
                    .Add("RepositoryCurrentState.Name", 1.0)
                    .Add("_id", 0.0))
            );

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = pipelineBsonDocuments.ToArray();            

            var options = new AggregateOptions()
            {
                AllowDiskUse = false
            };

            using (var cursor = await mongoCollection.AggregateAsync(pipeline, options))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        var jObject = JObject.Parse(document.ToJson());
                        var repositoryName = jObject["RepositoryCurrentState"][0]["Name"].Value<string>();

                        foundRepositoryNames.Add(repositoryName);
                    }
                }
            }

            return foundRepositoryNames;
        }
    }
}
