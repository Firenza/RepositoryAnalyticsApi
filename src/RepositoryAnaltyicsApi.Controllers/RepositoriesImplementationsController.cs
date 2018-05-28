using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using System;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories/{repositoryTypeName}/implementations")]
    public class RepositoriesImplementationsController : ControllerBase
    {
        private IRepositoryImplementationsManager repositoryImplementationsManager;

        public RepositoriesImplementationsController(IRepositoryImplementationsManager repositoryImplementationsManager)
        {
            this.repositoryImplementationsManager = repositoryImplementationsManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromRoute]string repositoryTypeName, [FromQuery]DateTime? createdOnOrAfter, [FromQuery]DateTime? createdOnOrBefore, [FromQuery]int? intervals)
        {
            var intervalCountAggregations =  await repositoryImplementationsManager.SearchAsync(repositoryTypeName, createdOnOrAfter, createdOnOrBefore, intervals);

            return new ObjectResult(intervalCountAggregations);
        }
    }
}
