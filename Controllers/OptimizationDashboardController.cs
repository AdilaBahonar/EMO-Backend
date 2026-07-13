using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.OptimizationDashboardDTOs;
using EMO.Models.DTOs.UserDTOs;
using EMO.Repositories.OptimizationDashboardRepo;
using EMO.Repositories.UserAccessRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class OptimizationDashboardController : ControllerBase
    {
        private readonly IOptimizationDashboardService optimizationDashboardService;
        private readonly IUserAccessService userAccessService;

        public OptimizationDashboardController(
            IOptimizationDashboardService optimizationDashboardService,
            IUserAccessService userAccessService)
        {
            this.optimizationDashboardService = optimizationDashboardService;
            this.userAccessService = userAccessService;
        }

        [HttpGet("{level}/{id}")]
        public async Task<IActionResult> Get(string level, Guid id, [FromQuery] OptimizationQueryParams q)
        {
            var normalizedLevel = (level ?? string.Empty).ToLowerInvariant();
            var allowed = new[] { "business", "facility", "building", "floor", "section", "office", "device", "sensor" };
            if (!allowed.Contains(normalizedLevel))
                return BadRequest("Unknown level. Use business|facility|building|floor|section|office|device|sensor.");

            var access = await CurrentAccessAsync();
            if (access is not null)
            {
                if (!access.IsLoginAllowed || !IsAllowed(access, normalizedLevel, id)) return Forbid();
                // Tenant aggregate optimization is supplied by the scoped Deep Dive and CRM APIs.
                // Direct optimization-detail calls are limited to an authorized office/device/sensor.
                if (access.IsTenant && normalizedLevel is not ("office" or "device" or "sensor")) return Forbid();
            }

            var includeBusinessSuggestions = access is null || !access.IsTenant;
            var response = await optimizationDashboardService.GetOptimizationDashboardAsync(
                normalizedLevel, id, q, includeBusinessSuggestions);
            return Ok(response);
        }

        private async Task<UserAccessScope?> CurrentAccessAsync()
        {
            var currentUser = HttpContext.Items["User"] as UserResponseDTO;
            return currentUser is not null && Guid.TryParse(currentUser.userId, out var userId)
                ? await userAccessService.GetByUserIdAsync(userId, HttpContext.RequestAborted)
                : null;
        }

        private static bool IsAllowed(UserAccessScope access, string level, Guid id)
        {
            if (access.HasGlobalAccess) return true;
            return level switch
            {
                "business" => access.BusinessIds.Contains(id),
                "facility" => access.FacilityIds.Contains(id),
                "building" => access.BuildingIds.Contains(id),
                "floor" => access.FloorIds.Contains(id),
                "section" => access.SectionIds.Contains(id),
                "office" => access.OfficeIds.Contains(id),
                "device" => access.DeviceIds.Contains(id),
                "sensor" => access.SensorIds.Contains(id),
                _ => false
            };
        }
    }
}
