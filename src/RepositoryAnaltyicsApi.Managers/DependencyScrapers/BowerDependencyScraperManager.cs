using Newtonsoft.Json.Linq;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers.Dependencies
{
    public class BowerDependencyScraperManager : IDependencyScraperManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public BowerDependencyScraperManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex => new Regex(@"bower\.json");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = await repositorySourceManager.ReadFilesAsync(owner, name, branch).ConfigureAwait(false);

            var bowerJsonFiles = files.Where(file => file.Name == "bower.json");

            if (bowerJsonFiles != null)
            {
                foreach (var bowerJsonFile in bowerJsonFiles)
                {
                    var bowerJsonContent = await repositorySourceManager.ReadFileContentAsync(owner, name, bowerJsonFile.FullPath).ConfigureAwait(false);

                    JObject jObject = null;
                    try
                    {
                        jObject = JObject.Parse(bowerJsonContent);
                    }
                    catch (Exception ex)
                    {
                        var exceptionMessage = $"Error parsing JSON from {owner} - {name} - {bowerJsonFile.FullPath}";
                        throw new ArgumentException(exceptionMessage, ex);
                    }

                    var bowerProdDependencies = jObject["dependencies"];

                    if (bowerProdDependencies != null)
                    {
                        foreach (var token in bowerProdDependencies)
                        {
                            var property = token as JProperty;

                            var dependency = new RepositoryDependency();
                            dependency.Environment = "Production";
                            dependency.Source = "bower";
                            dependency.Name = property.Name;
                            var cleansedVersionMatch = Regex.Match(property.Value.ToString(), @"[\d\.]+");
                            dependency.Version = cleansedVersionMatch.Value;
                            dependency.MajorVersion = Regex.Match(dependency.Version, @"\d+").Value;

                            dependencies.Add(dependency);
                        }
                    }

                    var bowerDevDependencies = jObject["devDependencies"];

                    if (bowerDevDependencies != null)
                    {
                        foreach (var token in bowerDevDependencies)
                        {
                            var property = token as JProperty;

                            var dependency = new RepositoryDependency();
                            dependency.Environment = "Development";
                            dependency.Source = "bower";
                            dependency.Name = property.Name;
                            var cleansedVersionMatch = Regex.Match(property.Value.ToString(), @"[\d\.]+");
                            dependency.Version = cleansedVersionMatch.Value;
                            dependency.MajorVersion = Regex.Match(dependency.Version, @"\d+").Value;

                            dependencies.Add(dependency);
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
