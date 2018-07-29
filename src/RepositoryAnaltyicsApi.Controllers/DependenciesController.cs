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
    [ApiController]
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
        public async Task<ActionResult<List<RepositoryDependencySearchResult>>> Get(string name, DateTime? asOf, string team, string topic, bool? hasContinuousDelivery)
        {
            var repositorySearch = new RepositorySearch
            {
                AsOf = asOf,
                HasContinuousDelivery = hasContinuousDelivery,
                Team = team,
                Topic = topic
            };

            var dependencySearchResults = await dependencyManager.SearchAsync(name, repositorySearch).ConfigureAwait(false);

            return dependencySearchResults;
        }
    }
}
