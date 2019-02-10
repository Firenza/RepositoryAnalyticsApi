using Dapper.Contrib.Extensions;
using RepositoryAnaltyicsApi.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RepositoryAnalyticsApi.Repositories.Model.MySql
{
    [Table("RepositoryDependencies")]
    public class RepositoryDependency
    {
        public static List<RepositoryDependency> MapFrom(ServiceModel.RepositorySnapshot repositorySnapshot, int repositorySnapshotId, IVersionManager versionManager)
        {
            var mappedRepositoryDependencies = new List<RepositoryDependency>();

            if (repositorySnapshot.Dependencies != null)
            {
                foreach (var dependency in repositorySnapshot.Dependencies)
                {
                    var mappedRepositoryDependency = new RepositoryDependency
                    {
                        RepositorySnapshotId = repositorySnapshotId,
                        Name = dependency.Name,
                        Version = dependency.Version,
                        MajorVersion = dependency.MajorVersion,
                        MinorVersion = versionManager.GetMinorVersion(dependency.Version),
                        PaddedVersion = versionManager.GetPaddedVersion(dependency.Version),
                        PreReleaseSemanticVersion = dependency.PreReleaseSemanticVersion,
                        Environment = dependency.Environment,
                        Source = dependency.Source,
                        RepoPath = dependency.RepoPath
                    };

                    mappedRepositoryDependencies.Add(mappedRepositoryDependency);
                }
            }

            return mappedRepositoryDependencies;
        }

        public int RepositorySnapshotId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        // The version number with padding to allow property sorting and filtering (E.G. 1.2.33 => 001.002.033)
        public string PaddedVersion { get; set; }
        public string MajorVersion { get; set; }
        public string MinorVersion { get; set; }
        /// <summary>
        /// Anything in the version string after a dash.  Stored here as the Version class doesn't know how to handle it
        /// </summary>
        public string PreReleaseSemanticVersion { get; set; }
        /// <summary>
        /// Where the dependency is needed. Mainly applies to NPM / Bower dependencies although NuGet does have this feature.
        /// </summary>
        public string Environment { get; set; }
        /// <summary>
        /// NPM, Bower, NuGet, Visual Studio Project File
        /// </summary>
        public string Source { get; set; }
        public string RepoPath { get; set; }
    }
}
