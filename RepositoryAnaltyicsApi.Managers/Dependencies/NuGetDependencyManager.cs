using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RepositoryAnaltyicsApi.Managers.Dependencies
{
    public class NuGetDependencyManager : IDependencyManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public NuGetDependencyManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public List<RepositoryDependency> Read(string repositoryId)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = repositorySourceManager.ReadFiles(repositoryId);

            // Check for .NET framework NuGet packages
            var packageConfigFiles = files.Where(file => file.Name == "packages.config");

            if (packageConfigFiles != null)
            {
                foreach (var packageConfigFile in packageConfigFiles)
                {
                    var packageConfigContent = repositorySourceManager.GetFileContent(repositoryId, packageConfigFile.FullPath);

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

            // Check for .NET standard NuGet packages
            var dotNetProjectFiles = files.Where(file => file.Name.EndsWith(".csproj") || file.Name.EndsWith(".vbproj"));

            if (dotNetProjectFiles != null && dotNetProjectFiles.Any())
            {
                foreach (var dotNetProjectFile in dotNetProjectFiles)
                {
                    var projectFileContent = repositorySourceManager.GetFileContent(repositoryId, dotNetProjectFile.FullPath);

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
                            var versionString = packageReferenceElement.LastAttribute.Value;
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
