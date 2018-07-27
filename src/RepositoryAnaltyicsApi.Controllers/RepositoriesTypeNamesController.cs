using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories/typeNames")]
    [ApiController]
    public class RepositoriesTypeNamesController : ControllerBase
    {
        private IRepositoriesTypeNamesManager repositoriesTypeNamesManager;

        public RepositoriesTypeNamesController(IRepositoriesTypeNamesManager repositoriesTypeNamesManager)
        {
            this.repositoriesTypeNamesManager = repositoriesTypeNamesManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetAsync(DateTime? asOf)
        {
            var typeNames = await repositoriesTypeNamesManager.ReadAsync(asOf);

            return typeNames;
        }
    }
}