using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryAnalysisManager
    {
        Task CreateAsync(RepositoryAnalysis repositoryAnalysis);
    }
}
