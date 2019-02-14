using System;

namespace RepositoryAnalyticsApi.ServiceModel
{
    [Serializable]
    public class RepositoryTeam
    {
        public string Name { get; set; }
        public string Permission { get; set; }
    }
}
