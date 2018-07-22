using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IDeriveRepositoryTypeAndImplementations
    {
        /// <summary>
        /// Derives repository type and implementation information
        /// </summary>
        Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName);
    }
}
