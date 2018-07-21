using GraphQl.NetStandard.Client;
using Newtonsoft.Json;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class User
    {
        public Repository Repository { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Repository>))]
        public GraphQlNodesParent<Repository> Repositories { get; set; }
    }
}
