using System.Collections.Generic;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class RepositoryImplementation
    {
        public static List<RepositoryImplementation> MapFrom(IEnumerable<ServiceModel.RepositoryImplementation> implementations, int repositoryTypeId)
        {
            var mappedImplementations = new List<RepositoryImplementation>();

            if (implementations != null)
            {
                foreach (var implementation in implementations)
                {
                    var mappedRepositoryFile = new RepositoryImplementation
                    {
                        RepositoryTypeId = repositoryTypeId,
                        Name = implementation.Name,
                        Version = implementation.Version,
                        MajorVersion = implementation.MajorVersion
                    };

                    mappedImplementations.Add(mappedRepositoryFile);
                }
            }

            return mappedImplementations;
        }

        public int Id { get; set; }
        public int RepositoryTypeId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public int? MajorVersion { get; set; }

    }
}
