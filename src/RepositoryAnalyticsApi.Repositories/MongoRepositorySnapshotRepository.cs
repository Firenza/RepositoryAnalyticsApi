using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        private IMongoCollection<RepositorySnapshot> mongoCollection;
        private IVersionManager versionPadManager;

        public MongoRepositorySnapshotRepository(IMongoCollection<RepositorySnapshot> mongoCollection, IVersionManager versionPadManager)
        {
            this.mongoCollection = mongoCollection;
            this.versionPadManager = versionPadManager;
        }

        public async Task UpsertAsync(RepositorySnapshot snapshot, int? repositoryCurrentStateId = null)
        {
            var filter = Builders<RepositorySnapshot>.Filter.And(
                Builders<RepositorySnapshot>.Filter.Eq(repo => repo.RepositoryCurrentStateId, snapshot.RepositoryCurrentStateId),
                Builders<RepositorySnapshot>.Filter.Eq(repo => repo.WindowStartCommitId, snapshot.WindowStartCommitId)
            );

            await mongoCollection.ReplaceOneAsync(filter, snapshot, new UpdateOptions { IsUpsert = true }).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string id)
        {
            await mongoCollection.DeleteOneAsync(repostiory => repostiory.WindowStartCommitId == id).ConfigureAwait(false);
        }

        public async Task<RepositorySnapshot> ReadAsync(string id)
        {
            var cursor = await mongoCollection.FindAsync(reposity => reposity.WindowStartCommitId == id).ConfigureAwait(false);
            var repository = await cursor.FirstOrDefaultAsync();

            return repository;
        }

        public async Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            var snapshots = new List<RepositorySnapshot>();

            var filter = Builders<RepositorySnapshot>.Filter.Eq(repo => repo.RepositoryCurrentStateId, repositoryCurrentStateId);

            using (var cursor = await mongoCollection.FindAsync<RepositorySnapshot>(filter).ConfigureAwait(false))
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
    }
}