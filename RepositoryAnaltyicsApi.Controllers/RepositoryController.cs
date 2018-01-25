using Microsoft.AspNetCore.Mvc;
using RepositoryAnalyticsApi.ServiceModel;
using System.Collections.Generic;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Route("api/[controller]")]
    public class RepositoryController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value2342342", "value2" };
        }

        [HttpGet("{id}")]
        public Repository Get(int id)
        {
            return new Repository();
        }

        [HttpPost]
        public void Post([FromBody]Repository value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Repository value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
