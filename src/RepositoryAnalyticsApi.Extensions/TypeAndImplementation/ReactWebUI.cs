using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class ReactWebUI : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var bowerAndNpmDependencies = dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

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
