using System;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class Commit
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime PushedDate { get; set; }
        public Tree Tree { get; set; }
    }
}
