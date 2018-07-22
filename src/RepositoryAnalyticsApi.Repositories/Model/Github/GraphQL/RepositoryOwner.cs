using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.Github.GraphQL
{
    [DataContract]
    public class RepositoryOwner
    {
        [DataMember(Name = "__typename")]
        public string TypeName { get; set; }
    }
}
