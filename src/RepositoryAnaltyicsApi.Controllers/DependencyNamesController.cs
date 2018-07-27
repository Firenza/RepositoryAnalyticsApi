using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Route("api/dependencies/names")]
    [ApiController]
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
        public async Task<ActionResult<List<string>>> Get(string nameRegex, DateTime? asOf)
        {
            var dependencies = await dependencyManager.SearchNamesAsync(nameRegex, asOf).ConfigureAwait(false);

            return dependencies;
        }
    }
}
