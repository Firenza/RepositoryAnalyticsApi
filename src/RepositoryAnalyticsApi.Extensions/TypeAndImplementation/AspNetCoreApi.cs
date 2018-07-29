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
    public class AspNetCoreApi : IDeriveRepositoryTypeAndImplementations, IRequireDependenciesAccess
    {
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }

        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName)
        {
            var nugetDependencies = Dependencies?.Where(dependency => dependency.Source == "NuGet");

            var aspNetCoreAllDependency = nugetDependencies.FirstOrDefault(dependency => dependency.Name == "Microsoft.AspNetCore.All");
            var aspNetCoreAppDependency = nugetDependencies.FirstOrDefault(dependency => dependency.Name == "Microsoft.AspNetCore.App");

            // If both are found use the newer metapackage
            var aspNetCoreDependency = aspNetCoreAppDependency ?? aspNetCoreAllDependency;

            if (aspNetCoreDependency != null)
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "API";

                typeAndImplementations.Implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "ASP.NET Core",
                        Version = aspNetCoreDependency.Version,
                        MajorVersion = aspNetCoreDependency.Version.GetMajorVersion()
                    }
                };

                return typeAndImplementations;
            }

            return null;
        }
    }
}
