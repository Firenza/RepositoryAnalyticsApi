namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// A DTO representing a GitHub repository
    /// </summary>
    public class Repository
    {
        public RepositoryCurrentState CurrentState { get; set; }
        public RepositorySnapshot Snapshot { get; set; }
    }
}
