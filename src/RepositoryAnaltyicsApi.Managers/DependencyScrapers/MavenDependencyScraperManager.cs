using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepositoryAnaltyicsApi.Managers.DependencyScrapers
{
    public class MavenDependencyScraperManager : IDependencyScraperManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public MavenDependencyScraperManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex =>new Regex(@"pom\.xml");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = await repositorySourceManager.ReadFilesAsync(owner, name, branch, asOf).ConfigureAwait(false);

            var mavenFiles = files.Where(file => file.Name == "pom.xml");

            if (mavenFiles != null)
            {
                foreach (var mavenFile in mavenFiles)
                {
                    var mavenFileContent = await repositorySourceManager.ReadFileContentAsync(owner, name, branch, mavenFile.FullPath, asOf).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(mavenFileContent))
                    {
                        string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                        if (mavenFileContent.StartsWith(byteOrderMarkUtf8))
                        {
                            mavenFileContent = mavenFileContent.Remove(0, byteOrderMarkUtf8.Length);

                            // For some reason removing the UTF preamble sometimes removes the first XML character
                            if (!mavenFileContent.StartsWith("<"))
                            {
                                mavenFileContent = $"<{mavenFileContent}";
                            }
                        }

                        var xDoc = XDocument.Parse(mavenFileContent);

                        var propertiesElement = xDoc.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == "properties");
                        var propertiesNameToValueMap = new Dictionary<string, string>();

                        if (propertiesElement != null)
                        {
                            foreach (var propertyElement in propertiesElement.Elements())
                            {
                                propertiesNameToValueMap.Add(propertyElement.Name.LocalName, propertyElement.Value);
                            }

                            // Replace any property names in the property values
                            foreach (var key in propertiesNameToValueMap.Keys.ToList())
                            {
                                var value = propertiesNameToValueMap[key];

                                var match = Regex.Match(value, @"\${(.*)}");

                                if (match.Success)
                                {
                                    propertiesNameToValueMap[key] = Regex.Replace(value, @"\${.*}", propertiesNameToValueMap[match.Groups[1].Value]);
                                }
                            }
                        }

                        var dependencyElements = xDoc.Descendants().Where(descendant => descendant.Name.LocalName == "dependency");

                        foreach (var dependencyElement in dependencyElements)
                        {
                            var repositoryDependency = new RepositoryDependency();

                            repositoryDependency.Name = dependencyElement.Elements().First(element => element.Name.LocalName == "artifactId").Value;

                            var match = Regex.Match(repositoryDependency.Name, @"\${(.*)}");

                            if (match.Success)
                            {
                                var propertyName = match.Groups[1].Value;
                                repositoryDependency.Name = Regex.Replace(repositoryDependency.Name, @"\${.*}", propertiesNameToValueMap[propertyName]);
                            }

                            repositoryDependency.Version = dependencyElement.Elements().First(element => element.Name.LocalName == "version").Value;

                            match = Regex.Match(repositoryDependency.Version, @"\${(.*)}");

                            if (match.Success)
                            {
                                var propertyName = match.Groups[1].Value;
                                repositoryDependency.Version = Regex.Replace(repositoryDependency.Version, @"\${.*}", propertiesNameToValueMap[propertyName]);
                            }

                            repositoryDependency.MajorVersion = Regex.Match(repositoryDependency.Version, @"\A\d+").Value;
                            repositoryDependency.RepoPath = mavenFile.FullPath;
                            repositoryDependency.Source = "Maven";

                            if (!dependencies.Any(
                                dependency => dependency.Name == repositoryDependency.Name && 
                                dependency.Version == repositoryDependency.Version && 
                                dependency.RepoPath == repositoryDependency.RepoPath))
                            {
                                dependencies.Add(repositoryDependency);
                            }
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
