namespace RepositoryAnalyticsApi.ServiceModel
{
    /// <summary>
    /// Generic DTO to return the results of a DB aggregation query
    /// </summary>
    public class CountAggregationResult
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}