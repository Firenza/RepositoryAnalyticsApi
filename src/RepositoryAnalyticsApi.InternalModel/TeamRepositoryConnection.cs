using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.InternalModel
{
    [Serializable]
    public class TeamRepositoryConnection
    {
        public string TeamPermissions { get; set; }
        public string RepositoryName { get; set; }
    }
}
