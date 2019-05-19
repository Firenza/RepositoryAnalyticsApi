using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/repoistories/{type}/implementations")]
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
        public async Task<ActionResult<List<IntervalCountAggregations>>> GetAsync(
            string type, 
            string team, 
            string topic, 
            bool? hasContinuousDelivery,
            DateTime? asOf, 
            DateTime?intervalStartTime, 
            DateTime? intervalEndTime, 
            int? intervals)
        {
            if (intervals.HasValue && (!intervalStartTime.HasValue && !intervalEndTime.HasValue))
            {
                var modelStateDictionary = new ModelStateDictionary();
                modelStateDictionary.TryAddModelError(nameof(intervals), "Can not specify an interval without a start or end time");

                return BadRequest(modelStateDictionary);
            }

            var repositorySearch = new RepositorySearch
            {
                TypeName = type,
                Team = team,
                Topic = topic,
                HasContinuousDelivery = hasContinuousDelivery,
                AsOf = asOf
            };

            var intervalInfo = new IntervalInfo
            {
                IntervalStartTime = intervalStartTime,
                IntervalEndTime = intervalEndTime,
                Intervals = intervals
            };

            var intervalCountAggregations =  await repositoryImplementationsManager.SearchAsync(repositorySearch, intervalInfo);

            return intervalCountAggregations;
        }
    }
}