using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MongoRepositoryRepository : IRepositoryRepository
    {
        public MongoRepositoryRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("local");
            var collection = db.GetCollection<Repository>("repository");
        }

        public void Create(Repository repository)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id)
        {
            throw new NotImplementedException();
        }

        public Repository Read(string id)
        {
            throw new NotImplementedException();
        }

        public void Update(Repository repository)
        {
            throw new NotImplementedException();
        }
    }
}
