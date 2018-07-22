using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class AspNetApi : IDeriveRepositoryTypeAndImplementations, IRequireDependenciesAccess, IRequireFileListAccess
    {
        public IEnumerable<RepositoryDependency> Dependencies { get; set; }
        public Func<Task<List<RepositoryFile>>> ReadFileListAsync { get; set; }

        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName)
        {
            var files = await ReadFileListAsync();

            // Check for ASP.NET based projects
            var globalAsaxFile = files.FirstOrDefault(file => file.Name == "Global.asax");

            if (globalAsaxFile != null)
            {
                var typeAndImplementations = new RepositoryTypeAndImplementations();
                typeAndImplementations.TypeName = "API";

                var implementations = new List<RepositoryImplementation>
                {
                    new RepositoryImplementation
                    {
                        Name = "ASP.NET"
                    }
                };
                
                // Can't use .NET path commands due to the unix style paths being used
                var fileNameRegex = @"[^/]+\Z";
                var globalAsaxDirectory = Regex.Replace(globalAsaxFile.FullPath, fileNameRegex, string.Empty);

                var nuGetDependencies = Dependencies?.Where(depencency => depencency.Source == "NuGet");

                var serviceStackDependency = nuGetDependencies.FirstOrDefault(dependency => dependency.RepoPath.Contains(globalAsaxDirectory) && dependency.Name == "ServiceStack");
                var webApiDependency = nuGetDependencies.FirstOrDefault(dependency => dependency.RepoPath.Contains(globalAsaxDirectory) && dependency.Name == "Microsoft.AspNet.WebApi");

                // Some WebApis bring in servicestack where as the opposite isn't true AFAIK
                if (serviceStackDependency != null && webApiDependency == null)
                {
                    implementations.Add(new RepositoryImplementation
                    {
                        Name = "ServiceStack",
                        Version = serviceStackDependency.Version,
                        MajorVersion = serviceStackDependency.Version.GetMajorVersion()
                    });

                    typeAndImplementations.Implementations = implementations;

                    return typeAndImplementations;
                }
                else if (webApiDependency != null)
                {
                    implementations.Add(new RepositoryImplementation
                    {
                        Name = "Web Api",
                        Version = webApiDependency.Version,
                        MajorVersion = webApiDependency.Version.GetMajorVersion()
                    });

                    typeAndImplementations.Implementations = implementations;

                    return typeAndImplementations;
                }
            }

            return null;
        }
    }
}
