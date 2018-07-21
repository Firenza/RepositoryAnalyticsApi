using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class Commit
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime PushedDate { get; set; }
    }
}
