using RepositoryAnalyticsApi.ServiceModel;

namespace RepositoryAnalyticsApi.Extensibility
{
    public interface IRequireBacklogInfoAccess
    {
        BacklogInfo BacklogInfo { get; set; }
    }
}
