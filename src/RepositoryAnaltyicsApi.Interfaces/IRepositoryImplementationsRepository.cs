using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryImplementationsRepository
    {
        Task<List<CountAggregationResult>> SearchAsync(RepositorySearch repositorySearch);
    }
}