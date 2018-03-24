using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoriesTypeNamesRepository
    {
        Task<List<string>> ReadAsync();
    }
}
