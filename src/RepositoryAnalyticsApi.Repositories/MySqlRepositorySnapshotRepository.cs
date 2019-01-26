using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositorySnapshotRepository : IRepositorySnapshotRepository
    {
        public MySqlRepositorySnapshotRepository(MySqlConnection mySqlConnection)
        {

        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<RepositorySnapshot>> ReadAllForParent(string repositoryCurrentStateId)
        {
            throw new NotImplementedException();
        }

        public Task<RepositorySnapshot> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(RepositorySnapshot snapshot)
        {
            throw new NotImplementedException();
        }
    }
}
