using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.OptimizationDashboardDTOs;
using EMO.Repositories.OptimizationDashboardRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class OptimizationDashboardController : ControllerBase
    {
        private readonly IOptimizationDashboardService optimizationDashboardService;

        public OptimizationDashboardController(IOptimizationDashboardService optimizationDashboardService)
        {
            this.optimizationDashboardService = optimizationDashboardService;
        }


        [HttpGet("{level}/{id}")]
        public async Task<IActionResult> Get(string level, Guid id, [FromQuery] OptimizationQueryParams q)
        {
            var allowed = new[] { "business", "facility", "building", "floor", "section", "office", "device", "sensor" };
            if (!allowed.Contains((level ?? string.Empty).ToLowerInvariant()))
                return BadRequest("Unknown level. Use business|facility|building|floor|section|office|device|sensor.");

            var response = await optimizationDashboardService.GetOptimizationDashboardAsync(level, id, q);
            return Ok(response);
        }

        //[HttpGet("{level}/{id}")]
        //public async Task<IActionResult> Get(string level, Guid id, [FromQuery] OptimizationQueryParams q)
        //{
        //    var allowed = new[] { "business", "facility", "building", "floor", "section", "office", "device", "sensor" };
        //    if (!allowed.Contains((level ?? string.Empty).ToLowerInvariant()))
        //        return BadRequest("Unknown level. Use business|facility|building|floor|section|office|device|sensor.");

        //    var response = await optimizationDashboardService.GetOptimizationDashboardAsync(level, id, q);
        //    return Ok(response);
        //}
    }
}
