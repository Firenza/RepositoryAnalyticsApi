using GraphQl.NetStandard.Client;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class CommitHistory
    {
        [JsonConverter(typeof(GraphQlNodesParentConverter<Commit>))]
        public GraphQlNodesParent<Commit> History { get; set; }
    }
}
