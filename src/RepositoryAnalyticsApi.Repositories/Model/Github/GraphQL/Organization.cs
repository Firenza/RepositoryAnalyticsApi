using GraphQl.NetStandard.Client;
using Newtonsoft.Json;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class Organization
    {
        public Repository Repository { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Repository>))]
        public GraphQlNodesParent<Repository> Repositories { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Team>))]
        public GraphQlNodesParent<Team> Teams { get; set; }
    }
}
