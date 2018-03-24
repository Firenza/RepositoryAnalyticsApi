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

        public async Task<List<IntervalCountAggregations>> SearchAsync(string typeName, DateTime? createdOnOrAfter, DateTime? createdOnOrBefore, int? intervals)
        {
            var intervalCountAggregations = new List<IntervalCountAggregations>();

            if (intervals.HasValue && intervals > 1)
            {
                if (!createdOnOrBefore.HasValue)
                {
                    createdOnOrBefore = DateTime.Now;
                }

                var intervalTimeSpan = (createdOnOrBefore.Value - createdOnOrAfter.Value) / intervals.Value;

                var searchTasks = new List<Task<IntervalCountAggregations>>();

                for (int i = 0; i < intervals.Value; i++)
                {
                    DateTime? intervalCreatedOnOrAfter = createdOnOrAfter.Value.Add(intervalTimeSpan * i);
                    DateTime? intervalCreatedOnOrBefore = intervalCreatedOnOrAfter.Value.Add(intervalTimeSpan);

                    var searchTask = repositoryImplementationsRepository.SearchAsync(typeName, intervalCreatedOnOrAfter, intervalCreatedOnOrBefore);

                    searchTasks.Add(searchTask);
                }

                await Task.WhenAll(searchTasks);

                foreach (var task in searchTasks)
                {
                    intervalCountAggregations.Add(task.Result);
                }
            }
            else
            {
                var intervalCountAggregation = await repositoryImplementationsRepository.SearchAsync(typeName, createdOnOrAfter, createdOnOrBefore);
                intervalCountAggregations.Add(intervalCountAggregation);
            }

            return intervalCountAggregations;
        }
    }
}
