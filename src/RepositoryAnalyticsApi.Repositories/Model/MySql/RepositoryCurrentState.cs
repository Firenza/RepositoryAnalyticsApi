using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class RepositoryCurrentState
    {
        public static RepositoryCurrentState MapFrom(RepositoryAnalyticsApi.ServiceModel.RepositoryCurrentState repositoryCurrentState)
        {
            return new RepositoryCurrentState
            {
                RepositoryId = repositoryCurrentState.Id,
                Name = repositoryCurrentState.Name,
                Owner = repositoryCurrentState.Owner,
                RepositoryCreatedOn = repositoryCurrentState.RepositoryCreatedOn,
                RepositoryLastUpdatedOn = repositoryCurrentState.RepositoryLastUpdatedOn,
                DefaultBranch = repositoryCurrentState.DefaultBranch,
                HasContinuousDelivery = repositoryCurrentState.DevOpsIntegrations?.ContinuousDelivery,
                HasContinuousDeployment = repositoryCurrentState.DevOpsIntegrations?.ContinuousDeployment,
                HasContinuousIntegration = repositoryCurrentState.DevOpsIntegrations?.ContinuousIntegration
            };
        }

        public string RepositoryId { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateTime? RepositoryCreatedOn { get; set; }
        public DateTime? RepositoryLastUpdatedOn { get; set; }
        public string DefaultBranch { get; set; }
        public bool? HasContinuousIntegration { get; set; }
        public bool? HasContinuousDelivery { get; set; }
        public bool? HasContinuousDeployment { get; set; }
    }
}
