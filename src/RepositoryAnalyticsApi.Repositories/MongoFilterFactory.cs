using MongoDB.Bson;
using MongoDB.Driver;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;

namespace RepositoryAnalyticsApi.Repositories
{
    public static class MongoFilterFactory
    {
        /// <summary>
        /// Generates BSON representing the RepositoryCurrenState filters matching the RepositoryFilter object
        /// </summary>
        /// <param name="repositorySearch"></param>
        /// <returns></returns>
        public static IEnumerable<BsonDocument> RepositoryCurrenStateFilters(RepositorySearch repositorySearch)
        {
            return RepositoryCurrenStateFilters(repositorySearch, false);
        }

        /// <summary>
        /// Generates BSON representing the RepositoryCurrenState filters matching the RepositoryFilter object. Adds a "RepositoryCurrentState." prefix to 
        /// filter names to reflect that a lookup was done with a destination object name of "RepositoryCurrenState"
        /// </summary>
        /// <param name="repositorySearch"></param>
        /// <returns></returns>
        public static IEnumerable<BsonDocument> RepositoryCurrenStatePostLookupFilters(RepositorySearch repositorySearch)
        {
            return RepositoryCurrenStateFilters(repositorySearch, true);
        }

        private static IEnumerable<BsonDocument> RepositoryCurrenStateFilters(RepositorySearch repositorySearch, bool isForPostLookupFiltering)
        {
            var bsonDocumentFilterList = new List<BsonDocument>();

            var postLookupFilterPrefix = string.Empty;

            if (isForPostLookupFiltering)
            {
                postLookupFilterPrefix = $"{nameof(RepositoryCurrentState)}.";
            }

            if (repositorySearch.HasContinuousDelivery.HasValue)
            {
                var fieldName = $"{postLookupFilterPrefix}{nameof(RepositoryCurrentState.DevOpsIntegrations)}.{nameof(RepositoryCurrentState.DevOpsIntegrations.ContinuousDelivery)}";

                bsonDocumentFilterList.Add(new BsonDocument()
                    .Add(fieldName, new BsonBoolean(repositorySearch.HasContinuousDelivery.Value))
                );
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.Topic))
            {
                bsonDocumentFilterList.Add(new BsonDocument()
                    .Add($"{postLookupFilterPrefix}{nameof(RepositoryCurrentState.Topics)}", repositorySearch.Topic)
                );
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.Team))
            {
                bsonDocumentFilterList.Add(new BsonDocument()
                    .Add($"{postLookupFilterPrefix}{nameof(RepositoryCurrentState.Teams)}", repositorySearch.Team)
                );
            }

            return bsonDocumentFilterList;
        }

        /// <summary>
        /// Generates BSON representing the RepositorySnapshot filters matching the RepositoryFilter object
        /// </summary>
        /// <param name="repositorySearch"></param>
        /// <returns></returns>
        public static IEnumerable<BsonDocument> RepositorySnapshotFilters(RepositorySearch repositorySearch)
        {
            var bsonDocumentFilterList = new List<BsonDocument>();

            if (!repositorySearch.AsOf.HasValue)
            {
                bsonDocumentFilterList.Add(new BsonDocument().Add(nameof(RepositorySnapshot.WindowEndsOn), BsonNull.Value));
            }
            else
            {
                bsonDocumentFilterList.Add(new BsonDocument()
                    .Add(nameof(RepositorySnapshot.WindowStartsOn), new BsonDocument()
                        .Add("$lte", new BsonDateTime(repositorySearch.AsOf.Value)))
                );
                bsonDocumentFilterList.Add(new BsonDocument().Add("$or", new BsonArray()
                    .Add(new BsonDocument()
                        .Add(nameof(RepositorySnapshot.WindowEndsOn), BsonNull.Value)
                     )
                     .Add(new BsonDocument()
                        .Add(nameof(RepositorySnapshot.WindowEndsOn), new BsonDocument()
                            .Add("$gte", new BsonDateTime(repositorySearch.AsOf.Value))
                        )
                     ))
                );
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.TypeName))
            {
                var fieldName = $"{nameof(RepositorySnapshot.TypesAndImplementations)}.{nameof(RepositoryTypeAndImplementations.TypeName)}";

                bsonDocumentFilterList.Add(new BsonDocument()
                  .Add(fieldName, repositorySearch.TypeName)
                );
            }

            if (!string.IsNullOrWhiteSpace(repositorySearch.ImplementationName))
            {
                var fieldName = $"{nameof(RepositorySnapshot.TypesAndImplementations)}.{nameof(RepositoryTypeAndImplementations.Implementations)}.{nameof(RepositoryImplementation.Name)}";

                bsonDocumentFilterList.Add(new BsonDocument()
                  .Add(fieldName, repositorySearch.ImplementationName)
                );
            }

            if (repositorySearch.Dependencies != null && repositorySearch.Dependencies.Any())
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

                    bsonDocumentFilterList
                        .Add(new BsonDocument()
                            .Add(nameof(RepositorySnapshot.Dependencies), new BsonDocument()
                                .Add("$elemMatch", dependencyElemMatchFilters)
                            )
                    );
                }
            }


            return bsonDocumentFilterList;
        }
    }
}
