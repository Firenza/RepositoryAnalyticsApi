using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repositorysource/repositories")]
    public class RepositorySourceRepositoriesController : ControllerBase
    {
        private IRepositorySourceManager repositorySourceManager;

        public RepositorySourceRepositoriesController(IRepositorySourceManager repositorySoruceManager)
        {
            this.repositorySourceManager = repositorySoruceManager;
        }
    
        /// <summary>
        /// Read repositories to analyze
        /// </summary>
        /// <param name="user"></param>
        /// <param name="organization"></param>
        /// <param name="take">Number of records to return</param>
        /// <param name="endCursor">Key denoting where the last request left off. This will be empty for the first request</param>
        /// <returns></returns>
        /// <remarks>
        /// Pages through the repositories from the source (E.G. GitHub) sorted by descending time of last update.  This route's main function is to
        /// facilitate processing of all repositories under a given organization or user.
        /// </remarks>
        [HttpGet()]
        public async Task<CursorPagedResults<RepositorySourceRepository>> Get([FromQuery] string user, [FromQuery] string organization, [FromQuery] int take, [FromQuery] string endCursor  )
        {
            var dependencies = await repositorySourceManager.ReadRepositoriesAsync(organization, user, take, endCursor);

            return dependencies;
        }
    }
}
