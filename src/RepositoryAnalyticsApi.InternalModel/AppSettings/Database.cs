using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.InternalModel.AppSettings
{
    public class Database
    {
        public string Type { get; set; }
        public string ConnectionString { get; set; }
    }
}
