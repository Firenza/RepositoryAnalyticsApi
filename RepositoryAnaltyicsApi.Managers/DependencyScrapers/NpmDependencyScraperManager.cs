using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers.Dependencies
{
    public class NpmDependencyScraperManager : IDependencyScraperManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public NpmDependencyScraperManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex => new Regex(@"package\.json");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = repositorySourceManager.ReadFiles(owner, name, branch);

            var packageJsonFile = files.FirstOrDefault(file => file.Name == "package.json");

            if (packageJsonFile != null)
            {
                var packageJsonContent = await repositorySourceManager.ReadFileContentAsync(owner, name, packageJsonFile.FullPath).ConfigureAwait(false);

                var jObject = JObject.Parse(packageJsonContent);

                var npmProdDependencies = jObject["dependencies"];

                if (npmProdDependencies != null)
                {
                    foreach (var token in npmProdDependencies)
                    {
                        var property = token as JProperty;

                        var dependency = new RepositoryDependency();
                        dependency.Environment = "Production";
                        dependency.Source = "npm";
                        dependency.Name = property.Name;
                        var cleansedVersionMatch = Regex.Match(property.Value.ToString(), @"[\d\.]+");
                        dependency.Version = cleansedVersionMatch.Value;
                        dependency.MajorVersion = Regex.Match(dependency.Version, @"\d+").Value;

                        dependencies.Add(dependency);
                    }
                }

                var npmDevDependencies = jObject["devDependencies"];


                if (npmDevDependencies != null)
                {
                    foreach (var token in npmDevDependencies)
                    {
                        var property = token as JProperty;

                        var dependency = new RepositoryDependency();
                        dependency.Environment = "Development";
                        dependency.Source = "npm";
                        dependency.Name = property.Name;
                        var cleansedVersionMatch = Regex.Match(property.Value.ToString(), @"[\d\.]+");
                        dependency.Version = cleansedVersionMatch.Value;
                        dependency.MajorVersion = Regex.Match(dependency.Version, @"\d+").Value;

                        dependencies.Add(dependency);
                    }
                }
            }

            return dependencies;
        }
    }
}
