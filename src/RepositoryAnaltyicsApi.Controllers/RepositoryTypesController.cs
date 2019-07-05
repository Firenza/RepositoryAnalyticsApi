using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repositories/types")]
    [ApiController]
    public class RepositoryTypesController : ControllerBase
    {
        private IRepositoryTypeManager repositoryTypeManager;

        public RepositoryTypesController(IRepositoryTypeManager repositoryTypeManager)
        {
            this.repositoryTypeManager = repositoryTypeManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<CountAggregationResult>>> GetAsync()
        {
            return await this.repositoryTypeManager.ReadAllAsync();
        }
    }
}
