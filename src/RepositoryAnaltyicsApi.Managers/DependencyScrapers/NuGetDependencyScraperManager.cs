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
    public class NuGetDependencyScraperManager : IDependencyScraperManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public NuGetDependencyScraperManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex => new Regex(@"\.csproj|\.vbproj|packages\.config");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch, DateTime? asOf)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = await repositorySourceManager.ReadFilesAsync(owner, name, branch, asOf).ConfigureAwait(false);

            // Check for .NET framework NuGet packages
            var packageConfigFiles = files.Where(file => file.Name == "packages.config");

            if (packageConfigFiles != null)
            {
                foreach (var packageConfigFile in packageConfigFiles)
                {
                    var packageConfigContent = await repositorySourceManager.ReadFileContentAsync(owner, name, branch, packageConfigFile.FullPath, asOf).ConfigureAwait(false);

                    string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                    if (packageConfigContent.StartsWith(byteOrderMarkUtf8))
                    {
                        packageConfigContent = packageConfigContent.Remove(0, byteOrderMarkUtf8.Length);

                        // For some reason removing the UTF preamble sometimes removes the first XML character
                        if (!packageConfigContent.StartsWith("<"))
                        {
                            packageConfigContent = $"<{packageConfigContent}";
                        }
                    }

                    var xdoc = XDocument.Parse(packageConfigContent);

                    foreach (var descendant in xdoc.Elements().First().Descendants())
                    {
                        var dependency = new RepositoryDependency();

                        dependency.RepoPath = packageConfigFile.FullPath;
                        dependency.Source = "NuGet";
                        dependency.Name = descendant.Attribute("id").Value;
                        var versionString = descendant.Attribute("version").Value;

                        var preReleaseSemanticVersionMatch = Regex.Match(versionString, @"-.*?\Z");

                        if (preReleaseSemanticVersionMatch.Success)
                        {
                            versionString = versionString.TrimEnd(preReleaseSemanticVersionMatch.Value.ToCharArray());
                            dependency.PreReleaseSemanticVersion = preReleaseSemanticVersionMatch.Value.TrimStart('-');
                        }

                        dependency.Version = versionString;
                        dependency.MajorVersion = Regex.Match(dependency.Version, @"\d+").Value;

                        dependencies.Add(dependency);
                    }
                }
            }

            // Check for Nuget packaes defined in project files from .NET Standard/Core projects OR from .NET Standard/Framework
            // dual targeted projects
            var dotNetProjectFiles = files.Where(file => file.Name.EndsWith(".csproj") || file.Name.EndsWith(".vbproj"));

            if (dotNetProjectFiles != null && dotNetProjectFiles.Any())
            {
                foreach (var dotNetProjectFile in dotNetProjectFiles)
                {
                    var projectFileContent = await repositorySourceManager.ReadFileContentAsync(owner, name, branch, dotNetProjectFile.FullPath, asOf).ConfigureAwait(false);

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

                    var packageReferenceElements = xDoc.Descendants().Where(descendant => descendant.Name.LocalName == "PackageReference");

                    if (packageReferenceElements != null)
                    {
                        foreach (var packageReferenceElement in packageReferenceElements)
                        {
                            var dependency = new RepositoryDependency();

                            dependency.RepoPath = dotNetProjectFile.FullPath;
                            dependency.Source = "NuGet";
                            dependency.Name = packageReferenceElement.FirstAttribute.Value;

                            string versionString = null;

                            // Figure out if this is a refernce from a .NET Standard/Core project file
                            if (packageReferenceElement.Attributes().Count() == 2 && packageReferenceElement.LastAttribute.Name == "Version")
                            {
                                versionString = packageReferenceElement.LastAttribute.Value;
                            }
                            // Figure out if this is a refernce from a .NET Framework project file that has been dual targeted with .NET standard
                            else if (packageReferenceElement.Attributes().Count() == 1 && packageReferenceElement.HasElements && packageReferenceElement.Elements().First().Name.LocalName == "Version")
                            {
                                versionString = packageReferenceElement.Elements().First().Value;
                            }

                            var preReleaseSemanticVersionMatch = Regex.Match(versionString, @"-.*?\Z");

                            if (preReleaseSemanticVersionMatch.Success)
                            {
                                versionString = versionString.TrimEnd(preReleaseSemanticVersionMatch.Value.ToCharArray());
                                dependency.PreReleaseSemanticVersion = preReleaseSemanticVersionMatch.Value.TrimStart('-');
                            }

                            dependency.Version = versionString;
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
