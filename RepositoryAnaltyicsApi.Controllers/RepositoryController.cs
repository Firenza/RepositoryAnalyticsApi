using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Route("api/[controller]")]
    public class RepositoryController : ControllerBase
    {
        private IRepositoryManager repositoryManager;

        public RepositoryController(IRepositoryManager repositoryManager)
        {
            this.repositoryManager = repositoryManager;
        }

        [HttpGet("{id}")]
        public async Task<Repository> Get(string id)
        {
            return await repositoryManager.ReadAsync(id).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task Post([FromBody]Repository value)
        {
            await repositoryManager.CreateAsync(value).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task Put([FromBody]Repository value)
        {
            await repositoryManager.UpdateAsync(value).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            await repositoryManager.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
