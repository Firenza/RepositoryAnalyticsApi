using Microsoft.AspNetCore.Mvc;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnalyticsApi.ServiceModel;
using System.Threading.Tasks;

namespace RepositoryAnaltyicsApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class RepositoryAnalysisController : ControllerBase
    {
        private IRepositoryAnalysisManager repositoryAnalysisManager;

        public RepositoryAnalysisController(IRepositoryAnalysisManager repositoryAnalysisManager)
        {
            this.repositoryAnalysisManager = repositoryAnalysisManager;
        }

        [HttpPost]
        [Route("api/[controller]")]
        public async Task Post(RepositoryAnalysis repositoyAnalysis)
        {
             await repositoryAnalysisManager.CreateAsync(repositoyAnalysis).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("api/[controller]/Existing")]
        public async Task<ReAnalysisResults> Post()
        {
            var reAnalysisResults = await repositoryAnalysisManager.ReAnalyzeExistingAsync().ConfigureAwait(false);

            return reAnalysisResults;
        }
    }
}
