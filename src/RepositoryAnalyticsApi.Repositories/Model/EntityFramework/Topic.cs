namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class Topic
    {
        public int TopicId { get; set; }
        public string Name { get; set; }

        public int RepositoryCurrentStateId { get; set; }
        public RepositoryCurrentState RepositoryCurrentState { get; set; }
    }
}
