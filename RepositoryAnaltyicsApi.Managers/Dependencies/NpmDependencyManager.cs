using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RepositoryAnaltyicsApi.Managers.Dependencies
{
    public class NpmDependencyManager : IDependencyManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public NpmDependencyManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public List<RepositoryDependency> Read(string repositoryId)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = repositorySourceManager.ReadFiles(repositoryId);

            var packageJsonFile = files.FirstOrDefault(file => file.Name == "package.json");

            if (packageJsonFile != null)
            {
                var packageJsonContent = repositorySourceManager.ReadFileContent(repositoryId, packageJsonFile.FullPath);

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
