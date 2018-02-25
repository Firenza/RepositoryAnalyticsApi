using RepositoryAnalyticsApi.Extensibliity;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class AspNetCoreApi : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var nugetDependencies = dependencies?.Where(dependency => dependency.Source == "nuget");

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
                    },
                    new RepositoryImplementation
                    {
                        Name = "Web API"
                    }
                };

                return typeAndImplementations;
            }

            return null;
        }
    }
}
