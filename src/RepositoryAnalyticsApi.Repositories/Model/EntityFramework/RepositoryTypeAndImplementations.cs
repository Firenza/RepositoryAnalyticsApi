using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryTypeAndImplementations
    {
        public int RepositoryTypeAndImplementationsId { get; set; }
        public string TypeName { get; set; }
        public IEnumerable<RepositoryImplementation> Implementations { get; set; }

        public int RepositorySnapshotId { get; set; }
        public RepositorySnapshot RepositorySnapshot { get; set; }
    }
}
