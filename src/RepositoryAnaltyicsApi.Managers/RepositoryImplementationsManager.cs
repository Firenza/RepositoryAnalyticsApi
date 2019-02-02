using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Managers
{
    public class RepositoryImplementationsManager : IRepositoryImplementationsManager
    {
        private IRepositoryImplementationsRepository repositoryImplementationsRepository;

        public RepositoryImplementationsManager(IRepositoryImplementationsRepository repositoryImplementationsRepository)
        {
            this.repositoryImplementationsRepository = repositoryImplementationsRepository;
        }

        public async Task<List<IntervalCountAggregations>> SearchAsync(RepositorySearch repositorySearch, IntervalInfo intervalInfo)
        {
            var intervalCountAggregations = new List<IntervalCountAggregations>();

            if (intervalInfo.Intervals.HasValue && intervalInfo.Intervals.Value > 1)
            {
                if (!intervalInfo.IntervalEndTime.HasValue)
                {
                    intervalInfo.IntervalEndTime = DateTime.Now;
                }

                var intervalTimeSpan = (intervalInfo.IntervalEndTime.Value - intervalInfo.IntervalStartTime.Value) / intervalInfo.Intervals.Value;

                var searchTasks = new List<Task<IntervalCountAggregations>>();

                for (int i = 0; i < intervalInfo.Intervals.Value; i++)
                {
                    DateTime? intervalCreatedOnOrAfter = intervalInfo.IntervalStartTime.Value.Add(intervalTimeSpan * i);
                    DateTime? intervalCreatedOnOrBefore = intervalCreatedOnOrAfter.Value.Add(intervalTimeSpan);

                    var searchTask = repositoryImplementationsRepository.SearchAsync(repositorySearch, intervalCreatedOnOrAfter, intervalCreatedOnOrBefore);

                    searchTasks.Add(searchTask);
                }

                await Task.WhenAll(searchTasks).ConfigureAwait(false);

                foreach (var task in searchTasks)
                {
                    intervalCountAggregations.Add(task.Result);
                }
            }
            else
            {
                var intervalCountAggregation = await repositoryImplementationsRepository.SearchAsync(repositorySearch, intervalInfo.IntervalStartTime, intervalInfo.IntervalEndTime).ConfigureAwait(false);
                intervalCountAggregations.Add(intervalCountAggregation);
            }

            return intervalCountAggregations;
        }
    }
}
