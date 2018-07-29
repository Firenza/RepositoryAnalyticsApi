using GraphQl.NetStandard.Client;
using Newtonsoft.Json;
using System;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    public class Repository
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
        // Forks will not have pushed date so this should be nullable
        public DateTime? PushedAt { get; set; }
        public Ref DefaultBranchRef { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Project>))]
        public GraphQlNodesParent<Project> Projects { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Issue>))]
        public GraphQlNodesParent<Issue> Issues { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<PullRequest>))]
        public GraphQlNodesParent<PullRequest> PullRequests { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Topic>))]
        public GraphQlNodesParent<Topic> RepositoryTopics { get; set; }
        public CommitHistory CommitHistory { get; set; }
        [JsonConverter(typeof(GraphQlNodesParentConverter<Ref>))]
        public GraphQlNodesParent<Ref> Refs { get; set; }
    }
}
