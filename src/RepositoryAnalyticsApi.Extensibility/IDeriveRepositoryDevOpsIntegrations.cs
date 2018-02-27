using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IDeriveRepositoryDevOpsIntegrations
    {
        /// <summary>
        /// Derives the level of DevOps integration the given repository has
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        Task<RepositoryDevOpsIntegrations> DeriveIntegrationsAsync(string repositoryName); 
    }
}
