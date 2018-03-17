using MongoDB.Bson;
using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryRepository : IRepositoriesRepository
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

        public async Task<List<Repository>> SearchAsync(string typeName, string implementationName, List<(string Name, string Version)> dependencies)
        {
            var foundRepositories = new List<Repository>();

            BsonDocument filter = new BsonDocument();
            var filterArray = new BsonArray();

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                filterArray.Add(new BsonDocument()
                               .Add(nameof(Repository.TypesAndImplementations), new BsonDocument()
                                       .Add("$elemMatch", new BsonDocument()
                                               .Add(nameof(RepositoryTypeAndImplementations.TypeName), typeName)
                                       )
                               )
                       );
            }

            if (!string.IsNullOrWhiteSpace(implementationName))
            {
                filterArray.Add(new BsonDocument()
                               .Add($"{nameof(Repository.TypesAndImplementations)}.{nameof(RepositoryTypeAndImplementations.Implementations)}", new BsonDocument()
                                       .Add("$elemMatch", new BsonDocument()
                                               .Add(nameof(RepositoryImplementation.Name), implementationName)
                                       )
                               )
                       );
            }

            if (dependencies.Any())
            {
                foreach (var dependency in dependencies)
                {
                    filterArray.Add(new BsonDocument()
                            .Add(nameof(Repository.Dependencies), new BsonDocument()
                                    .Add("$elemMatch", new BsonDocument()
                                            .Add(nameof(RepositoryDependency.Name), dependency.Name)
                                            .Add(nameof(RepositoryDependency.Version), new BsonRegularExpression($"^{dependency.Version}", "i"))
                                    )
                            )
                    );
                }
            }

            filter.Add("$and", filterArray);

            BsonDocument projection = new BsonDocument();
            projection.Add("Name", 1.0);

            var options = new FindOptions<Repository>()
            {
                Projection = projection
            };

            using (var cursor = await mongoCollection.FindAsync(filter, options))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (Repository repository in batch)
                    {
                        foundRepositories.Add(repository);
                    }
                }
            }

            return foundRepositories;
        }

        public async Task UpdateAsync(Repository repository)
        {
            await mongoCollection.ReplaceOneAsync(repo => repo.Id == repository.Id, repository);

        }
    }
}