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
    public class DotNetProjectFileDependencyManager : IDependencyManager
    {
        private IRepositorySourceManager repositorySourceManager;

        public DotNetProjectFileDependencyManager(IRepositorySourceManager repositorySourceManager)
        {
            this.repositorySourceManager = repositorySourceManager;
        }

        public Regex SourceFileRegex => new Regex(@"\.csproj|\.vbproj");

        public async Task<List<RepositoryDependency>> ReadAsync(string owner, string name, string branch)
        {
            var dependencies = new List<RepositoryDependency>();

            var files = this.repositorySourceManager.ReadFiles(owner, name, branch);

            var dotNetProjectFiles = files.Where(file => file.Name.EndsWith(".csproj") || file.Name.EndsWith(".vbproj"));

            if (dotNetProjectFiles != null && dotNetProjectFiles.Any())
            {
                foreach (var dotNetProjectFile in dotNetProjectFiles)
                {
                    var projectFileContent = await this.repositorySourceManager.ReadFileContentAsync(owner, name, dotNetProjectFile.FullPath).ConfigureAwait(false);

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
                            var dotNetStandardVersion = Regex.Match(targetFrameworkElement.Value, @"(?:\d|\.)+").Value;

                            var dotNetStandardDependency = new RepositoryDependency();
                            dotNetStandardDependency.Name = ".NET Standard";
                            dotNetStandardDependency.Version = dotNetStandardVersion;
                            dotNetStandardDependency.MajorVersion = Regex.Match(dotNetStandardDependency.Version, @"\d+").Value;
                            dotNetStandardDependency.Environment = "Production";
                            dotNetStandardDependency.Source = "Visual Studio Project File";
                            dotNetStandardDependency.RepoPath = dotNetProjectFile.FullPath;

                            dependencies.Add(dotNetStandardDependency);
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
