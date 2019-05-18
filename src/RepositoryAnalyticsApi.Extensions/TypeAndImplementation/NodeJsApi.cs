using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class NodeJsApi : IDeriveRepositoryTypeAndImplementations, IRequireDependenciesAccess
    {
        public IEnumerable<RepositoryDependency> Dependencies {get; set; }

        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName)
        {
            var bowerAndNpmDependencies = Dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

            if (bowerAndNpmDependencies != null && bowerAndNpmDependencies.Any())
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "API";
                var implementations = new List<RepositoryImplementation>();

                var expressDepencency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "express");
                var hapiDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "hapi");

                if (expressDepencency != null)
                {
                    implementations.Add(new RepositoryImplementation
                    {
                        Name = "Node JS - Express",
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
                        Name = "Node JS - Hapi",
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
