using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories")]
    public class RepositoriesController : ControllerBase
    {
        private IRepositoriesManager repositoriesManager;
        // Get any non digit portion at the start of the version number.  If there is one assume it's a range
        // specifier like >=
        private string rangeSpecifierRegex = @"^[^\d]+";

        public RepositoriesController(IRepositoriesManager repositoriesManager)
        {
            this.repositoriesManager = repositoriesManager;
        }

        /// <summary>
        /// Search repositories
        /// </summary>
        /// <param name="typeName">Exact match</param>
        /// <param name="implementationName">Exact match</param>
        /// <param name="dependencies">Exact match on name</param>
        /// <remarks>Dependency matches can contain both a name and partial or complete version number.  E.G. ".NET Framework:4" matches any .NET Framework
        /// dependencies with a major version of 4 and ".NET Framework:4.5.2" matches .NET Framework dependencies with version 4.5.2</remarks>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]string typeName, [FromQuery]string implementationName, [FromQuery]List<string> dependencies, [FromQuery]bool? hasContinuousDelivery)
        {
            var parsedDependencies = new List<(string Name, string Version, RangeSpecifier RangeSpecifier)>();

            if (dependencies.Any())
            {
                foreach (var dependency in dependencies)
                {
                    var dependencyParts = dependency.Split(':');

                    var dependencyName = dependencyParts[0];
                    string dependencyVersion = null;

                    if (dependencyParts.Length == 2)
                    {
                        dependencyVersion = dependencyParts[1];
                    }

                    if (string.IsNullOrWhiteSpace(dependencyVersion))
                    {
                        parsedDependencies.Add((dependencyParts[0], null, RangeSpecifier.Unspecified));
                    }
                    else
                    {
                        var rangeSpecifier = RangeSpecifier.Unspecified;

                        var match = Regex.Match(dependencyVersion, rangeSpecifierRegex);

                        if (match.Success)
                        {
                            switch (match.Value)
                            {
                                case ">=":
                                    dependencyVersion = dependencyVersion.Replace(">=", "");
                                    rangeSpecifier = RangeSpecifier.GreaterThanOrEqualTo;
                                    break;
                                case ">":
                                    dependencyVersion = dependencyVersion.Replace(">", "");
                                    rangeSpecifier = RangeSpecifier.GreaterThan;
                                    break;
                                case "<":
                                    dependencyVersion = dependencyVersion.Replace("<", "");
                                    rangeSpecifier = RangeSpecifier.LessThan;
                                    break;
                                case "<=":
                                    dependencyVersion = dependencyVersion.Replace("<=", "");
                                    rangeSpecifier = RangeSpecifier.LessThanOrEqualTo;
                                    break;
                            }
                        }

                        parsedDependencies.Add((dependencyName, dependencyVersion, rangeSpecifier));
                    }
                }
            }

            var repositorySearch = new RepositorySearch
            {
                TypeName = typeName,
                ImplementationName = implementationName,
                HasContinuousDelivery = hasContinuousDelivery,
                Dependencies = parsedDependencies
            };

            var repositories = await repositoriesManager.SearchAsync(repositorySearch);

            return new ObjectResult(repositories);
        }
    }
}
