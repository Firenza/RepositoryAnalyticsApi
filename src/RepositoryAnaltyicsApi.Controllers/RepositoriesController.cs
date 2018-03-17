using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories")]
    public class RepositoriesController : ControllerBase
    {
        private IRepositoriesManager repositoriesManager;

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
        public async Task<IActionResult> GetAsync([FromQuery]string typeName, [FromQuery]string implementationName, [FromQuery]List<string> dependencies)
        {
            var parsedDependencies = new List<(string name, string version)>();

            if (dependencies.Any())
            {
                foreach (var dependency in dependencies)
                {
                    var dependencyParts = dependency.Split(':');

                    if (dependencyParts.Length == 1)
                    {
                        parsedDependencies.Add((dependencyParts[0], null));
                    }
                    else if (dependencyParts.Length == 2)
                    {
                        parsedDependencies.Add((dependencyParts[0], dependencyParts[1]));
                    }
                }
            }

            var repositories = await repositoriesManager.SearchAsync(typeName, implementationName,  parsedDependencies);

            return new ObjectResult(repositories);
        }
    }
}
