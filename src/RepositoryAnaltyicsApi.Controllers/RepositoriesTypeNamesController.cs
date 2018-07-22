using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories/typeNames")]
    public class RepositoriesTypeNamesController : ControllerBase
    {
        private IRepositoriesTypeNamesManager repositoriesTypeNamesManager;

        public RepositoriesTypeNamesController(IRepositoriesTypeNamesManager repositoriesTypeNamesManager)
        {
            this.repositoriesTypeNamesManager = repositoriesTypeNamesManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]DateTime? asOf)
        {
            var typeNames = await repositoriesTypeNamesManager.ReadAsync(asOf);

            return new ObjectResult(typeNames);
        }
    }
}