﻿using RepositoryAnalyticsApi.ServiceModel;
using System;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryManager
    {
        void Create(Repository repository);
        Repository Read(string id);
        void Update(Repository repository);
        void Delete(string id);
    }
}