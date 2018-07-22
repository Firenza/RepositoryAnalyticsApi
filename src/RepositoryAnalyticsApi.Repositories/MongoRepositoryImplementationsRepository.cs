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

        public async Task<IntervalCountAggregations> SearchAsync(string typeName, DateTime? createdOnOrAfter, DateTime? createdOnOrBefore)
        {
            var options = new AggregateOptions()
            {
                AllowDiskUse = false
            };

            var matchStatements = new BsonArray();

            matchStatements.Add(new BsonDocument().Add("TypesAndImplementations.TypeName", typeName));

            if (createdOnOrAfter.HasValue)
            {
                matchStatements.Add(new BsonDocument().Add("WindowStartsOn", new BsonDocument().Add("$lte", new BsonDateTime(createdOnOrAfter.Value))));
            }

            if (createdOnOrBefore.HasValue)
            {
                matchStatements.Add(new BsonDocument().Add("WindowEndsOn", new BsonDocument().Add("$gte", new BsonDateTime(createdOnOrBefore.Value))));
            }

            // If not interval window specified then just read from the most recent snapshots
            if (!createdOnOrAfter.HasValue && !createdOnOrBefore.HasValue)
            {
                matchStatements.Add(new BsonDocument().Add("WindowEndsOn", BsonNull.Value));
            }

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument()
                        .Add("$and", matchStatements)),
                new BsonDocument("$unwind", new BsonDocument()
                        .Add("path", "$TypesAndImplementations")
                        .Add("includeArrayIndex", "arrayIndex")
                        .Add("preserveNullAndEmptyArrays", new BsonBoolean(false))),
                new BsonDocument("$project", new BsonDocument()
                        .Add("TypeAndImplementations", "$TypesAndImplementations")),
                new BsonDocument("$match", new BsonDocument()
                        .Add("TypeAndImplementations.TypeName", typeName)),
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
                        ))
            };

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
