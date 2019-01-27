using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    public class RepositoryFile
    {
        public static List<RepositoryFile> MapFrom(ServiceModel.RepositorySnapshot repositorySnapshot, int repositorySnapshotId)
        {
            var mappedRepositoryFiles = new List<RepositoryFile>();

            if (repositorySnapshot.Files != null)
            {
                foreach (var file in repositorySnapshot.Files)
                {
                    var mappedRepositoryFile = new RepositoryFile
                    {
                        RepositorySnapshotId = repositorySnapshotId,
                        Name = file.Name,
                        FullPath = file.FullPath
                    };

                    mappedRepositoryFiles.Add(mappedRepositoryFile);
                }
            }

            return mappedRepositoryFiles;
        }

        public int RepositorySnapshotId { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
    }
}
