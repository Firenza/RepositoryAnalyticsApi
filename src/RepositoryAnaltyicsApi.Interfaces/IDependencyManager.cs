﻿using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IDependencyManager
    {
        Task<List<string>> SearchNamesAsync(string name);
        Task<List<RepositoryDependencySearchResult>> SearchAsync(string name);
    }
}