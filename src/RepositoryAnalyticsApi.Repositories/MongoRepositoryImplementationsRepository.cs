using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryImplementationsRepository : IRepositoryImplementationsRepository
    {
        private IMongoCollection<BsonDocument> mongoCollection;

        public MongoRepositoryImplementationsRepository(IMongoCollection<BsonDocument> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task<IntervalCountAggregations> SearchAsync(RepositorySearch repositorySearch, DateTime? createdOnOrAfter, DateTime? createdOnOrBefore)
        {
            var options = new AggregateOptions()
            {
                AllowDiskUse = false
            };

            var preLookupSnapshotFilterArray = new BsonArray();
            var repositorySnapshotFilters = MongoFilterFactory.RepositorySnapshotFilters(repositorySearch);
            preLookupSnapshotFilterArray.AddRange(repositorySnapshotFilters);

            var postLookupCurrentStateFilterArray = new BsonArray();
            var repositoryCurrenStateFilters = MongoFilterFactory.RepositoryCurrenStatePostLookupFilters(repositorySearch);
            postLookupCurrentStateFilterArray.AddRange(repositoryCurrenStateFilters);

            var pipelineBsonDocuments = new List<BsonDocument>();

            // Can always add this since there will always be a window filter query
            pipelineBsonDocuments.Add(
                new BsonDocument("$match", new BsonDocument()
                    .Add("$and", preLookupSnapshotFilterArray))
            );

            if (postLookupCurrentStateFilterArray.Count > 0)
            {
                pipelineBsonDocuments.AddRange(new List<BsonDocument>
                {
                    new BsonDocument("$lookup", new BsonDocument()
                      .Add("from", "repositoryCurrentState")
                      .Add("localField", "RepositoryCurrentStateId")
                      .Add("foreignField", "_id")
                      .Add("as", "RepositoryCurrentState")),
                  new BsonDocument("$match", new BsonDocument()
                    .Add("$and", postLookupCurrentStateFilterArray))

                });
            }

            pipelineBsonDocuments.AddRange(new List<BsonDocument> {
                new BsonDocument("$unwind", new BsonDocument()
                    .Add("path", "$TypesAndImplementations")
                    .Add("includeArrayIndex", "arrayIndex")
                    .Add("preserveNullAndEmptyArrays", new BsonBoolean(false))),
                new BsonDocument("$match", new BsonDocument()
                  .Add("$and", preLookupSnapshotFilterArray)),
                new BsonDocument("$project", new BsonDocument()
                  .Add("TypeAndImplementations", "$TypesAndImplementations")),
                new BsonDocument("$match", new BsonDocument()
                  .Add("TypeAndImplementations.TypeName", repositorySearch.TypeName)),
                new BsonDocument("$unwind", new BsonDocument()
                    .Add("path", "$TypeAndImplementations.Implementations")
                    .Add("includeArrayIndex", "arrayIndex")
                    .Add("preserveNullAndEmptyArrays", new BsonBoolean(false))),
                 new BsonDocument("$group", new BsonDocument()
                    .Add("_id", new BsonDocument()
                        .Add("Implementation", "$TypeAndImplementations.Implementations.Name")
                     )
                     .Add("count", new BsonDocument()
                        .Add("$sum", 1.0)
                     )
                ),
                new BsonDocument("$sort", new BsonDocument()
                        .Add("_id.Implementation", 1.0))
            });

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = pipelineBsonDocuments.ToArray();

            var intervalCountAggregation = new IntervalCountAggregations();
            intervalCountAggregation.IntervalStart = createdOnOrAfter;
            intervalCountAggregation.IntervalEnd = createdOnOrBefore;
            intervalCountAggregation.CountAggreations = new List<CountAggreation>();

            using (var cursor = await mongoCollection.AggregateAsync(pipeline, options))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        var jObject = JObject.Parse(document.ToJson());

                        var countAggregation = new CountAggreation();
                        countAggregation.Name = jObject["_id"]["Implementation"].Value<string>();
                        countAggregation.Count = jObject["count"].Value<int>();

                        intervalCountAggregation.CountAggreations.Add(countAggregation);
                    }
                }
            }

            return intervalCountAggregation;
        }
    }
}
