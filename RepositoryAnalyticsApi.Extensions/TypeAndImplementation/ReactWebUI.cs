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
    public class ReactWebUI : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<(string TypeName, IEnumerable<RepositoryImplementation> Implemementations)> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var bowerAndNpmDependencies = dependencies?.Where(dependency => dependency.Source == "bower" || dependency.Source == "npm");

            // Check for Angular 1.x
            var reactDependency = bowerAndNpmDependencies.FirstOrDefault(dependency => dependency.Name == "react");

            if (reactDependency != null)
            {
                var typeName = "Web UI";

                var implementation = new RepositoryImplementation();
                implementation.Name = "React";
                implementation.Version = reactDependency.Version;
                implementation.MajorVersion = reactDependency.Version.GetMajorVersion();

                return (typeName, new List<RepositoryImplementation> { implementation });
            }

            return (null, null);
        }
    }
}
