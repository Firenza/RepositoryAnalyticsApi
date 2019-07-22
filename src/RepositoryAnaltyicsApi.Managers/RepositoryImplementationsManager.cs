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

        public async Task<List<CountAggregationResult>> SearchAsync(RepositorySearch repositorySearch)
        {
            var countAggregations = await repositoryImplementationsRepository.SearchAsync(repositorySearch).ConfigureAwait(false);

            return countAggregations;
        }
    }
}
