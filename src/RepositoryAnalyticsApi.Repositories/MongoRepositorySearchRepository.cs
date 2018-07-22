using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
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

            BsonDocument filter = new BsonDocument();
            var filterArray = new BsonArray();

            if (!repositorySearch.AsOf.HasValue)
            {
                filterArray.Add(new BsonDocument().Add(nameof(RepositorySnapshot.WindowEndsOn), BsonNull.Value));
            }
            else
            {
                filterArray.Add(new BsonDocument().Add("WindowEndsOn", new BsonDocument().Add("$lte", new BsonDateTime(repositorySearch.AsOf.Value))));
                filterArray.Add(new BsonDocument().Add("WindowStartsOn", new BsonDocument().Add("$gte", new BsonDateTime(repositorySearch.AsOf.Value))));
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.TypeName))
            {
                filterArray
                    .Add(new BsonDocument()
                       .Add(nameof(RepositorySnapshot.TypesAndImplementations), new BsonDocument()
                           .Add("$elemMatch", new BsonDocument()
                               .Add(nameof(RepositoryTypeAndImplementations.TypeName), repositorySearch.TypeName)
                           )
                       )
                    );
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.ImplementationName))
            {
                filterArray
                    .Add(new BsonDocument()
                      .Add($"{nameof(RepositorySnapshot.TypesAndImplementations)}.{nameof(RepositoryTypeAndImplementations.Implementations)}", new BsonDocument()
                          .Add("$elemMatch", new BsonDocument()
                               .Add(nameof(RepositoryImplementation.Name), repositorySearch.ImplementationName)
                               )
                          )
                    );
            }

            if (repositorySearch.Dependencies.Any())
            {
                foreach (var dependency in repositorySearch.Dependencies)
                {
                    var dependencyElemMatchFilters = new BsonDocument();

                    dependencyElemMatchFilters.Add(nameof(RepositoryDependency.Name), dependency.Name);

                    if (!string.IsNullOrWhiteSpace(dependency.Version))
                    {
                        BsonValue versionFilter = null;

                        if (dependency.RangeSpecifier == RangeSpecifier.Unspecified)
                        {
                            var regexEscapedDependencyVersion = dependency.Version.Replace(".", @"\.");

                            versionFilter = new BsonRegularExpression($"^{regexEscapedDependencyVersion}", "i");
                        }
                        else if (!string.IsNullOrWhiteSpace(dependency.Version))
                        {
                            switch (dependency.RangeSpecifier)
                            {
                                case RangeSpecifier.GreaterThan:
                                    versionFilter = new BsonDocument().Add("$gt", dependency.Version);
                                    break;
                                case RangeSpecifier.GreaterThanOrEqualTo:
                                    versionFilter = new BsonDocument().Add("$gte", dependency.Version);
                                    break;
                                case RangeSpecifier.LessThan:
                                    versionFilter = new BsonDocument().Add("$lt", dependency.Version);
                                    break;
                                case RangeSpecifier.LessThanOrEqualTo:
                                    versionFilter = new BsonDocument().Add("$lte", dependency.Version);
                                    break;
                            }
                        }

                        dependencyElemMatchFilters.Add(nameof(RepositoryDependency.Version), versionFilter);
                    }

                    filterArray
                        .Add(new BsonDocument()
                            .Add(nameof(RepositorySnapshot.Dependencies), new BsonDocument()
                                .Add("$elemMatch", dependencyElemMatchFilters)
                            )
                    );
                }
            }

            if (repositorySearch.HasContinuousDelivery.HasValue)
            {
                filterArray
                    .Add(new BsonDocument()
                       .Add("DevOpsIntegrations.ContinuousDelivery", new BsonBoolean(true)
                    )
                );
            }

            filter.Add("$and", filterArray);

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filter),
                new BsonDocument("$lookup", new BsonDocument()
                        .Add("from", "repositoryCurrentState")
                        .Add("localField", "RepositoryCurrentStateId")
                        .Add("foreignField", "_id")
                        .Add("as", "RepositoryCurrentState")),
                new BsonDocument("$project", new BsonDocument()
                        .Add("RepositoryCurrentState.Name", 1.0)
                        .Add("_id", 0.0))
            };

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
