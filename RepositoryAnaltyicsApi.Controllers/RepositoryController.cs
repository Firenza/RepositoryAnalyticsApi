using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;

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
        public Repository Get(string id)
        {
            return repositoryManager.Read(id);
        }

        [HttpPost]
        public void Post([FromBody]Repository value)
        {
            repositoryManager.Create(value);
        }

        [HttpPut]
        public void Put([FromBody]Repository value)
        {
            repositoryManager.Update(value);
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            repositoryManager.Delete(id);
        }
    }
}
