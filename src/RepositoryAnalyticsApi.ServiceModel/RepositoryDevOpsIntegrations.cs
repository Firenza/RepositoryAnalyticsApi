using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.ServiceModel
{
    [Serializable]
    public class RepositoryDevOpsIntegrations
    {
        public bool ContinuousIntegration { get; set; }
        public bool ContinuousDelivery { get; set; }
        public bool ContinuousDeployment { get; set; }
    }
}