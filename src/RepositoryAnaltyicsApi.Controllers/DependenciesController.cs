using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/dependencies")]
    public class DependenciesController : ControllerBase
    {
        private IDependencyManager dependencyManager;

        public DependenciesController(IDependencyManager dependencyManager)
        {
            this.dependencyManager = dependencyManager;
        }

        /// <summary>
        /// Search dependencies
        /// </summary>
        /// <param name="name">Must be an exact match</param>
        /// <returns></returns>
        /// <remarks>
        /// Search dependency information by dependency name.  Results are grouped by Name and Version.
        /// </remarks>
        [HttpGet()]
        public async Task<List<RepositoryDependencySearchResult>> Get([FromQuery] string name, [FromQuery]DateTime? asOf)
        {
            return await dependencyManager.SearchAsync(name, asOf).ConfigureAwait(false);
        }
    }
}
