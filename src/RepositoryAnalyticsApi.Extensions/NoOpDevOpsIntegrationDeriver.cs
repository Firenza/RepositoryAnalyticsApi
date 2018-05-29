using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions
{
    /// <summary>
    /// A no op implementation of IDeriveRepositoryDevOpsIntegrations to put into the container if ther are no others available
    /// </summary>
    public class NoOpDevOpsIntegrationDeriver : IDeriveRepositoryDevOpsIntegrations
    {
        public Task<RepositoryDevOpsIntegrations> DeriveIntegrationsAsync(string repositoryName)
        {
            return null;
        }
    }
}