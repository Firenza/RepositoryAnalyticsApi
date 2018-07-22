using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class ReactWebUI : IDeriveRepositoryTypeAndImplementations, IRequireDependenciesAccess
    {
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }

        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName)
        {
            var bowerAndNpmDependencies = Dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

            var reactDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "react");

            if (reactDependency != null)
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "Web UI";

                typeAndImplementations.Implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "React",
                        Version = reactDependency.Version,
                        MajorVersion = reactDependency.Version.GetMajorVersion()
                    }
                };

                return typeAndImplementations;
            }

            return null;
        }
    }
}
