using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

                }
            }

            return dependencies;
        }
    }
}
