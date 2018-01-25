using MongoDB.Driver;
using RepositoryAnalyticsApi.ServiceModel;
using System;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryRepository
    {
        public MongoRepositoryRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("local");
            var collection = db.GetCollection<Repository>("repository");
        }
    }
}
