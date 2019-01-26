using MySql.Data.MySqlClient;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Repositories
{
    public class MySqlRepositoryCurrentStateRepository : IRepositoryCurrentStateRepository
    {
        public MySqlRepositoryCurrentStateRepository(MySqlConnection mySqlConnection)
        {

        }

        public Task UpsertAsync(RepositoryCurrentState repository)
        {
            throw new NotImplementedException();
        }
    }
}
