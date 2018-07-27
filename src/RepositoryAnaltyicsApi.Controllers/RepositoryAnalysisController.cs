using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryAnalysisController : ControllerBase
    {
        private IRepositoryAnalysisManager repositoryAnalysisManager;

        public RepositoryAnalysisController(IRepositoryAnalysisManager repositoryAnalysisManager)
        {
            this.repositoryAnalysisManager = repositoryAnalysisManager;
        }

        [HttpPost]
        public async Task Post(RepositoryAnalysis repositoyAnalysis)
        {
             await repositoryAnalysisManager.CreateAsync(repositoyAnalysis).ConfigureAwait(false);
        }
    }
}
