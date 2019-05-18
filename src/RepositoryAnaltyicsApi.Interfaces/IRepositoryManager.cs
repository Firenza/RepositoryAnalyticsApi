﻿using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryManager
    {
        Task UpsertAsync(Repository repository, DateTime? asOf);
        Task<Repository> ReadAsync(string id, DateTime? asOf);
        Task<List<Repository>> ReadMultipleAsync(DateTime? asOf, int? page, int? pageSize);
        Task<List<string>> SearchAsync(RepositorySearch repositorySearch);
    }
}
