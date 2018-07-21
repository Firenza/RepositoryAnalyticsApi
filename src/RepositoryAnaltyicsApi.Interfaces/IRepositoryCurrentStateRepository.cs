using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryCurrentStateRepository
    {
        Task UpsertAsync(RepositoryCurrentState repository);
    }
}
