using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/dependencies/names")]
    public class DependencyNamesController : ControllerBase
    {
        private IDependencyManager dependencyManager;

        public DependencyNamesController(IDependencyManager dependencyManager)
        {
            this.dependencyManager = dependencyManager;
        }

        /// <summary>
        /// Search dependency names
        /// </summary>
        /// <param name="nameRegex">A regex to match names on</param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<List<string>> Get([FromQuery] string nameRegex)
        {
            var dependencies = await dependencyManager.SearchNamesAsync(nameRegex).ConfigureAwait(false);

            return dependencies;
        }
    }
}
