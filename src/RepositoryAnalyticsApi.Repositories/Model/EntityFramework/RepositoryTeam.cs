namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryTeam
    {
        public int RepositoryTeamId { get; set; }
        public string Name { get; set; }
        public string Permission { get; set; }

        public int RepositoryCurrentStateId { get; set; }
        public RepositoryCurrentState RepositoryCurrentState { get; set; }
    }
}
