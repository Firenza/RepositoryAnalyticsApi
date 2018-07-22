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

            if (aspNetCoreAllDependency != null)
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "API";

                typeAndImplementations.Implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "ASP.NET Core",
                        Version = aspNetCoreAllDependency.Version,
                        MajorVersion = aspNetCoreAllDependency.Version.GetMajorVersion()
                    }
                };

                return typeAndImplementations;
            }

            return null;
        }
    }
}
