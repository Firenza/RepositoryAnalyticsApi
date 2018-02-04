using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Route("api/[controller]")]
    public class DependenciesController : ControllerBase
    {
        private IDependencyManager dependencyManager;

        public DependenciesController(IDependencyManager dependencyManager)
        {
            this.dependencyManager = dependencyManager;
        }

        [HttpGet()]
        public async Task<List<RepositoryDependency>> Get([FromQuery] string name)
        {
            var dependencies = await dependencyManager.SearchAsync(name).ConfigureAwait(false);

            return dependencies;
        }
    }
}
