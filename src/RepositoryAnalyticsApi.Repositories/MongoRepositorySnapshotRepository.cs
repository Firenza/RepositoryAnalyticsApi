﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private IMongoCollection<RepositorySnapshot> mongoCollection;

        public MongoRepositorySnapshotRepository(IMongoCollection<RepositorySnapshot> mongoCollection)
        {
            this.mongoCollection = mongoCollection;
        }

        public async Task UpsertAsync(RepositorySnapshot snapshot)
        {
            var filter = Builders<RepositorySnapshot>.Filter.And(
                Builders<RepositorySnapshot>.Filter.Eq(repo => repo.RepositoryCurrentStateId, snapshot.RepositoryCurrentStateId),
                Builders<RepositorySnapshot>.Filter.Eq(repo => repo.WindowStartCommitId, snapshot.WindowStartCommitId)
            );

            await mongoCollection.ReplaceOneAsync(filter, snapshot, new UpdateOptions { IsUpsert = true });
        }

        public async Task DeleteAsync(string id)
        {
            await mongoCollection.DeleteOneAsync(repostiory => repostiory.WindowStartCommitId == id);
        }

        public async Task<RepositorySnapshot> ReadAsync(string id)
        {
            var cursor = await mongoCollection.FindAsync(reposity => reposity.WindowStartCommitId == id);
            var repository = await cursor.FirstOrDefaultAsync();

            return repository;
        }

        public async Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            var snapshots = new List<RepositorySnapshot>();

            var filter = Builders<RepositorySnapshot>.Filter.Eq(repo => repo.RepositoryCurrentStateId, repositoryCurrentStateId);

            using (var cursor = await mongoCollection.FindAsync<RepositorySnapshot>(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (RepositorySnapshot repository in batch)
                    {
                        snapshots.Add(repository);
                    }
                }
            }

            return snapshots;
        }

        public async Task<List<RepositorySnapshot>> SearchAsync(RepositorySearch repositorySearch)
        {
            var foundRepositories = new List<RepositorySnapshot>();

            BsonDocument filter = new BsonDocument();
            var filterArray = new BsonArray();

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

            BsonDocument projection = new BsonDocument();
            projection.Add("Name", 1.0);

            var options = new FindOptions<RepositorySnapshot>()
            {
                Projection = projection
            };

            using (var cursor = await mongoCollection.FindAsync(filter, options))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (RepositorySnapshot repository in batch)
                    {
                        foundRepositories.Add(repository);
                    }
                }
            }

            return foundRepositories;
        }
    }
}