using System.Runtime.Serialization;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    [DataContract]
    public class RepositoryOwner
    {
        [DataMember(Name = "__typename")]
        public string TypeName { get; set; }
    }
}
