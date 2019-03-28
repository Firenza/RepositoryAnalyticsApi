namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryImplementation
    {
        public int RepositoryImplementationId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public int? MajorVersion { get; set; }


        public int RepositoryTypeAndImplementationsId { get; set; }
        public RepositoryTypeAndImplementations RepositoryTypeAndImplementations { get; set; }
    }
}
