using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Interfaces
{
    public interface IRepositoriesTypeNamesManager
    {
        Task<List<string>> ReadAsync();
    }
}
