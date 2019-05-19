namespace RepositoryAnalyticsApi.InternalModel.AppSettings
{
    public class Dependencies
    {
        public GitHub GitHub { get; set; }
        public Database Database { get; set; }
        public ElasticSearch ElasticSearch { get; set; }
        public Redis Redis { get; set; }
    }
}
