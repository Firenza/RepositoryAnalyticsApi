using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class RepositoryType
    {
        public static List<RepositoryType> MapFrom(ServiceModel.RepositorySnapshot repositorySnapshot, int repositorySnapshotId)
        {
            var mappedTypes = new List<RepositoryType>();

            if (repositorySnapshot.TypesAndImplementations != null)
            {
                foreach (var typeAndImplementation in repositorySnapshot.TypesAndImplementations)
                {
                    var mappedRepositoryType = new RepositoryType
                    {
                        RepositorySnapshotId = repositorySnapshotId,
                        Name = typeAndImplementation.TypeName
                    };
             

                    mappedTypes.Add(mappedRepositoryType);
                }
            }

            return mappedTypes;
        }

        public int Id { get; set; }
        public int RepositorySnapshotId { get; set; }
        public string Name { get; set; }
    }
}
