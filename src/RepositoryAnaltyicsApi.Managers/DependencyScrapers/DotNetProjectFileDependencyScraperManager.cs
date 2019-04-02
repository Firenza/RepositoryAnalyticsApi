using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepositoryAnaltyicsApi.Managers.Dependencies
{
    public class DotNetProjectFileDependencyScraperManager : IDependencyScraperManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public DotNetProjectFileDependencyScraperManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex => new Regex(@"\.csproj|\.vbproj");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = await repositorySourceManager.ReadFilesAsync(owner, name, branch, asOf).ConfigureAwait(false);

            var dotNetProjectFiles = files.Where(file => file.Name.EndsWith(".csproj") || file.Name.EndsWith(".vbproj"));

            if (dotNetProjectFiles != null && dotNetProjectFiles.Any())
            {
                foreach (var dotNetProjectFile in dotNetProjectFiles)
                {
                    var projectFileContent = await this.repositorySourceManager.ReadFileContentAsync(owner, name, branch, dotNetProjectFile.FullPath, asOf).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(projectFileContent))
                    {
                        string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                        if (projectFileContent.StartsWith(byteOrderMarkUtf8))
                        {
                            projectFileContent = projectFileContent.Remove(0, byteOrderMarkUtf8.Length);

                            // For some reason removing the UTF preamble sometimes removes the first XML character
                            if (!projectFileContent.StartsWith("<"))
                            {
                                projectFileContent = $"<{projectFileContent}";
                            }
                        }

                        var xDoc = XDocument.Parse(projectFileContent);

                        // Look for a .NET framework element
                        var targetFrameworkVersionElement = xDoc.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == "TargetFrameworkVersion");

                        if (targetFrameworkVersionElement != null)
                        {
                            var dotNetFrameworkVersion = targetFrameworkVersionElement.Value.TrimStart('v');

                            var dotNetFrameworkDependency = new RepositoryDependency();
                            dotNetFrameworkDependency.Name = ".NET Framework";
                            dotNetFrameworkDependency.Version = dotNetFrameworkVersion;
                            dotNetFrameworkDependency.MajorVersion = Regex.Match(dotNetFrameworkDependency.Version, @"\d+").Value;
                            dotNetFrameworkDependency.Environment = "Production";
                            dotNetFrameworkDependency.Source = "Visual Studio Project File";
                            dotNetFrameworkDependency.RepoPath = dotNetProjectFile.FullPath;

                            dependencies.Add(dotNetFrameworkDependency);
                        }
                        else
                        {
                            // Look for a .NET core / standard element
                            var targetFrameworkElement = xDoc.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == "TargetFramework");

                            if (targetFrameworkElement != null)
                            {
                                // AFAIK only these two names are used in this element
                                //netstandard
                                //netcoreapp

                                var match = Regex.Match(targetFrameworkElement.Value, @"([A-z]+)([\d|\.]+)");

                                var appType = match.Groups[1].Value;
                                var version = match.Groups[2].Value;

                                var netCoreStandardDependency = new RepositoryDependency();
                                if (appType.ToLower() == "netstandard")
                                {
                                    netCoreStandardDependency.Name = ".NET Standard";
                                }
                                else if (appType.ToLower() == "netcoreapp")
                                {
                                    netCoreStandardDependency.Name = ".NET Core";
                                }
                                else
                                {
                                    throw new ArgumentException($"Unrecognized .NET application type of {appType}");
                                }

                                netCoreStandardDependency.Version = version;
                                netCoreStandardDependency.MajorVersion = Regex.Match(netCoreStandardDependency.Version, @"\d+").Value;
                                netCoreStandardDependency.Source = "Visual Studio Project File";
                                netCoreStandardDependency.RepoPath = dotNetProjectFile.FullPath;

                                dependencies.Add(netCoreStandardDependency);
                            }
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
