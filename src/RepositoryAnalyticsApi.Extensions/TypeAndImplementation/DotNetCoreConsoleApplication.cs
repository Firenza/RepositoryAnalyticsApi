using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepositoryAnalyticsApi.Extensions.TypeAndImplementation
{
    [Export(typeof(IDeriveRepositoryTypeAndImplementations))]
    public class DotNetCoreConsoleApplication : IDeriveRepositoryTypeAndImplementations, IRequireFileListAccess, IRequireFileContentAccess
    {
        public Func<Task<List<RepositoryFile>>> ReadFileListAsync { get; set; }
        public Func<string, Task<string>> ReadFileContentAsync { get; set; }

        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(string repositoryName)
        {
            var files = await ReadFileListAsync();
            var projFiles = files.Where(file => file.FullPath.EndsWith(".csproj") || file.FullPath.EndsWith(".vbproj"));

            foreach (var projFile in projFiles)
            {
                var projectFileContent = await ReadFileContentAsync(projFile.FullPath).ConfigureAwait(false);

                var xDoc = XmlHelper.RemoveUtf8ByteOrderMarkAndParse(projectFileContent);

                var targetFrameworkElement = xDoc.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == "TargetFramework");
                var outputTypeElement = xDoc.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == "OutputType");

                var isNetCoreApplication = targetFrameworkElement != null && targetFrameworkElement.Value.StartsWith("netcoreapp");
                var isExeOutputType = outputTypeElement != null && outputTypeElement.Value == "Exe";

                if (isNetCoreApplication && isExeOutputType)
                {
                    var typeAndImplementations = new RepositoryTypeAndImplementations();
                    typeAndImplementations.TypeName = "Console Application";

                    typeAndImplementations.Implementations = new List<RepositoryImplementation>
                    {
                        new RepositoryImplementation
                        {
                            Name = ".NET Core",
                        }
                    };

                    return typeAndImplementations;
                }
            }

            return null;
        }
    }
}
