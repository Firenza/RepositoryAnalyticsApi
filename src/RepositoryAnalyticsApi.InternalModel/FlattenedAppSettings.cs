namespace RepositoryAnalyticsApi.InternalModel
{
    public class FlattenedAppSettings
    {
        public int CachingDurationRepositoryData { get; set; }
        public int CachingDurationOrganizationTeams { get; set; }
        public int CachingDurationOwnerType { get; set; }
        public int CachingDurationDevOpsIntegrations { get; set; }
        public string GitHubV3ApiUrl { get; set; }
        public string GitHubGraphQlApiUrl { get; set; }
        public string ElasticSearchUrl { get; set; }
        public string RedisConfiguration { get; set; }
        public string RedisInstanceName { get; set; }
        public string DatabaseType { get; set; }
        public string DatabaseConnectionString { get; set; }
    }
}