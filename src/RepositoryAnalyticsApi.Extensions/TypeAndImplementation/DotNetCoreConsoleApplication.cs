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
    public class DotNetCoreConsoleApplication : IDeriveRepositoryTypeAndImplementations
    {
        public async Task<RepositoryTypeAndImplementations> DeriveImplementationAsync(IEnumerable<RepositoryDependency> dependencies, Func<Task<List<RepositoryFile>>> readFilesAsync, IEnumerable<string> topics, string name, Func<string, Task<string>> readFileContentAsync)
        {
            var files = await readFilesAsync();
            var projFiles = files.Where(file => file.FullPath.EndsWith(".csproj") || file.FullPath.EndsWith(".vbproj"));

            foreach (var projFile in projFiles)
            {
                var projectFileContent = await readFileContentAsync(projFile.FullPath).ConfigureAwait(false);

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
