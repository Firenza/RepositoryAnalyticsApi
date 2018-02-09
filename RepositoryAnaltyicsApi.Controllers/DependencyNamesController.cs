using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Route("api/dependencies/names")]
    public class DependencyNamesController : ControllerBase
    {
        private IDependencyManager dependencyManager;

        public DependencyNamesController(IDependencyManager dependencyManager)
        {
            this.dependencyManager = dependencyManager;
        }

        [HttpGet()]
        public async Task<List<string>> Get([FromQuery] string partialName)
        {
            var dependencies = await dependencyManager.SearchNamesAsync(partialName).ConfigureAwait(false);

            return dependencies;
        }
    }
}
