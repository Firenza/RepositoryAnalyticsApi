using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repositories/{type}/implementations")]
    [ApiController]
    public class RepositoriesImplementationsController : ControllerBase
    {
        private IRepositoryImplementationsManager repositoryImplementationsManager;

        public RepositoriesImplementationsController(IRepositoryImplementationsManager repositoryImplementationsManager)
        {
            this.repositoryImplementationsManager = repositoryImplementationsManager;
        }

        /// <summary>
        /// Search for repository implementations
        /// </summary>
        /// <param name="repositoryTypeName"></param>
        /// <param name="intervalStartTime">Interval start time</param>
        /// <param name="intervalEndTime">Interval end time</param>
        /// <param name="intervals"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CountAggregationResult>>> GetAsync(
            string type, 
            string team, 
            string topic, 
            bool? hasContinuousDelivery)
        {
      
            var repositorySearch = new RepositorySearch
            {
                TypeName = type,
                Team = team,
                Topic = topic,
                HasContinuousDelivery = hasContinuousDelivery,
            };

   
            var countAggregations =  await repositoryImplementationsManager.SearchAsync(repositorySearch);

            return countAggregations;
        }
    }
}