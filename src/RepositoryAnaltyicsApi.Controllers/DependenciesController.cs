using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/dependencies/{name}")]
    [ApiController]
    public class DependenciesController : ControllerBase
    {
        private IDependencyManager dependencyManager;

        public DependenciesController(IDependencyManager dependencyManager)
        {
            this.dependencyManager = dependencyManager;
        }

        /// <summary>
        /// Read dependency information
        /// </summary>
        /// <param name="name">Must be an exact match</param>
        /// <returns></returns>
        /// <remarks>
        /// Read infomration about a specific dependency.  Results are grouped by version
        /// </remarks>
        [HttpGet()]
        public async Task<ActionResult<List<RepositoryDependencySearchResult>>> Get(string name, DateTime? asOf, string team, string topic, bool? hasContinuousDelivery)
        {
            var repositorySearch = new RepositorySearch
            {
                AsOf = asOf,
                HasContinuousDelivery = hasContinuousDelivery,
                Team = team,
                Topic = topic
            };

            var dependencySearchResults = await dependencyManager.ReadAsync(name, repositorySearch).ConfigureAwait(false);

            return dependencySearchResults;
        }
    }
}
