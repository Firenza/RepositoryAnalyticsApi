﻿namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class TeamToRepositoryEdge
    {
        public string Permission { get; set; }
        public Repository Repository { get; set; }
    }
}
