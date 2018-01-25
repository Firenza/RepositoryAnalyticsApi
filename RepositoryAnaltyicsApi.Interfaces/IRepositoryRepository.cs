using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryRepository
    {
        void Create(Repository repository);
        Repository Read(string id);
        void Update(Repository repository);
        void Delete(string id);
    }
}
