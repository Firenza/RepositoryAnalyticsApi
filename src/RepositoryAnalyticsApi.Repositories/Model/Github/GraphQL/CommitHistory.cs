using GraphQl.NetStandard.Client;
using Newtonsoft.Json;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class CommitHistory
    {
        [JsonConverter(typeof(GraphQlNodesParentConverter<Commit>))]
        public GraphQlNodesParent<Commit> History { get; set; }
    }
}
