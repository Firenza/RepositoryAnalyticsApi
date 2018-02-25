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
    public class NodeJsApi : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var bowerAndNpmDependencies = dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

            if (bowerAndNpmDependencies != null && bowerAndNpmDependencies.Any())
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "API";

                var implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "Node JS"
                    }
                };

                var expressDepencency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "exoress");
                var hapiDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "hapi");

                if (expressDepencency != null)
                {
                    implementations.Add(new RepositoryImplementation
                    {
                        Name = "express",
                        Version = expressDepencency.Version,
                        MajorVersion = expressDepencency.Version.GetMajorVersion()
                    });

                    typeAndImplementations.Implementations = implementations;

                    return typeAndImplementations;
                }
                else if (hapiDependency != null)
                {
                    implementations.Add(new RepositoryImplementation
                    {
                        Name = "hapi",
                        Version = hapiDependency.Version,
                        MajorVersion = hapiDependency.Version.GetMajorVersion()
                    });

                    typeAndImplementations.Implementations = implementations;

                    return typeAndImplementations;
                }
            }

            return null;
        }
    }
}
