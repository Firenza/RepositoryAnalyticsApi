namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryFile
    {
        public int RepositoryFileId { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }

        public int RepositorySnapshotId { get; set; }
        public RepositorySnapshot RepositorySnapshot { get; set; }
    }
}
