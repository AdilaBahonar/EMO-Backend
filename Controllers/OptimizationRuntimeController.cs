using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.OptimizationRuntimeDTOs;
using EMO.Repositories.OptimizationRuntimeRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    //[ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class OptimizationRuntimeController : ControllerBase
    {
        private readonly IOptimizationRuntimeService service;

        public OptimizationRuntimeController(IOptimizationRuntimeService service)
        {
            this.service = service;
        }

        [HttpGet("Configuration/{businessId:guid}")]
        public async Task<IActionResult> Configuration(Guid businessId)
            => Ok(await service.GetConfigurationAsync(businessId));

        [HttpPost("Suggestions/Sync")]
        public async Task<IActionResult> SyncSuggestions(
            [FromBody] OptimizationSuggestionSyncRequestDTO request,
            CancellationToken cancellationToken)
            => Ok(await service.SyncSuggestionsAsync(request, cancellationToken));
    }
}
