using RepositoryAnalyticsApi.Extensibliity;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class AngularWebUI : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var bowerAndNpmDependencies = dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

            // Check for Angular 1.x
            var angularDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "angular");

            if (angularDependency == null)
            {
                // Check for Angular 2.x and higher
                angularDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "@angular/core");
            }

            if (angularDependency != null)
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "Web UI";

                typeAndImplementations.Implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "Angular",
                        Version = angularDependency.Version,
                        MajorVersion = angularDependency.Version.GetMajorVersion()
                    }
                };

                return typeAndImplementations;
            }

            return null;
        }
    }
}
