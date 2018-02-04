using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoryAnalysisManager
    {
        Task CreateAsync(string repositoryUrl);
    }
}
