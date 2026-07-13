using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.UserDTOs;
using EMO.Repositories.EnergyDashboardServicesRepo;
using EMO.Repositories.UserAccessRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyDashboardController : ControllerBase
    {
        private readonly IEnergyDashboardService energyDashboardService;
        private readonly IUserAccessService userAccessService;

        public EnergyDashboardController(
            IEnergyDashboardService energyDashboardService,
            IUserAccessService userAccessService)
        {
            this.energyDashboardService = energyDashboardService;
            this.userAccessService = userAccessService;
        }

        [HttpGet("GetMonthlyDeviceTypeReport")]
        public async Task<ActionResult<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>>> GetMonthlyDeviceTypeReport(
            string? businessId = null,
            string? tenantId = null)
        {
            var scope = await ResolveLegacyScopeAsync(businessId, tenantId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetMonthlyDeviceTypeReport(scope.BusinessId, scope.TenantId));
        }

        [HttpGet("GetEnergyConsumptionByDeviceTypeLast12Months")]
        public async Task<ActionResult<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>>> GetEnergyConsumptionByDeviceTypeLast12Months(
            string? businessId = null,
            string? tenantId = null)
        {
            var scope = await ResolveLegacyScopeAsync(businessId, tenantId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetEnergyConsumptionByDeviceTypeLast12Months(scope.BusinessId, scope.TenantId));
        }

        [HttpGet("GetPeakNonPeakAnalysis")]
        public async Task<ActionResult<ResponseModel<PeakNonPeakSummaryResponseDTO>>> GetPeakNonPeakAnalysis(
            DateTime startDate,
            DateTime endDate,
            string? businessId = null,
            string? tenantId = null)
        {
            var scope = await ResolveLegacyScopeAsync(businessId, tenantId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetPeakNonPeakAnalysis(
                startDate, endDate, scope.BusinessId, scope.TenantId));
        }

        [HttpGet("ExportPeakNonPeakAnalysisCsv")]
        public async Task<IActionResult> ExportPeakNonPeakAnalysisCsv(
            DateTime startDate,
            DateTime endDate,
            string? businessId = null,
            string? tenantId = null)
        {
            var scope = await ResolveLegacyScopeAsync(businessId, tenantId);
            if (!scope.Allowed) return Forbid();
            var fileBytes = await energyDashboardService.ExportPeakNonPeakAnalysisCsv(
                startDate, endDate, scope.BusinessId, scope.TenantId);
            return File(fileBytes, "text/csv", "peak-non-peak-analysis.csv");
        }

        [HttpGet("ExportEnergyConsumptionByDeviceTypeCsv")]
        public async Task<IActionResult> ExportEnergyConsumptionByDeviceTypeCsv(
            string? businessId = null,
            string? tenantId = null)
        {
            var scope = await ResolveLegacyScopeAsync(businessId, tenantId);
            if (!scope.Allowed) return Forbid();
            var fileBytes = await energyDashboardService.ExportEnergyConsumptionByDeviceTypeCsv(
                scope.BusinessId, scope.TenantId);
            return File(fileBytes, "text/csv", "energy-consumption-by-device-type.csv");
        }

        [HttpGet("crm/business/summary")]
        public async Task<ActionResult<ResponseModel<CrmDashboardSummaryResponseDTO>>> GetBusinessDashboardSummary(Guid businessId)
        {
            if (!await CanAccessBusinessAsync(businessId)) return Forbid();
            return Ok(await energyDashboardService.GetBusinessDashboardSummary(businessId));
        }

        [HttpGet("crm/business/live-overview")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ResponseModel<CrmDashboardLiveOverviewResponseDTO>>> GetBusinessDashboardLiveOverview(
            Guid businessId,
            bool forceRefresh = false)
        {
            if (!await CanAccessBusinessAsync(businessId)) return Forbid();
            return Ok(await energyDashboardService.GetBusinessDashboardLiveOverview(businessId, forceRefresh));
        }

        [HttpGet("crm/business/chart")]
        public async Task<ActionResult<ResponseModel<CrmDashboardChartResponseDTO>>> GetBusinessDashboardChart(
            Guid businessId,
            string chartType = "energyconsumption",
            string range = "30d",
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (!await CanAccessBusinessAsync(businessId)) return Forbid();
            return Ok(await energyDashboardService.GetBusinessDashboardChart(
                businessId, chartType, range, fromDate, toDate));
        }

        [HttpGet("crm/business/suggestions")]
        public async Task<ActionResult<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>>> GetBusinessDashboardSuggestions(Guid businessId)
        {
            if (!await CanAccessBusinessAsync(businessId)) return Forbid();
            return Ok(await energyDashboardService.GetBusinessDashboardSuggestions(businessId));
        }

        [HttpGet("crm/tenant/summary")]
        public async Task<ActionResult<ResponseModel<CrmDashboardSummaryResponseDTO>>> GetTenantDashboardSummary(
            Guid tenantId,
            Guid? businessId = null)
        {
            var scope = await ResolveTenantScopeAsync(tenantId, businessId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetTenantDashboardSummary(scope.TenantId, scope.BusinessId));
        }

        [HttpGet("crm/tenant/live-overview")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ResponseModel<CrmDashboardLiveOverviewResponseDTO>>> GetTenantDashboardLiveOverview(
            Guid tenantId,
            Guid? businessId = null,
            bool forceRefresh = false)
        {
            var scope = await ResolveTenantScopeAsync(tenantId, businessId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetTenantDashboardLiveOverview(
                scope.TenantId, scope.BusinessId, forceRefresh));
        }

        [HttpGet("crm/tenant/chart")]
        public async Task<ActionResult<ResponseModel<CrmDashboardChartResponseDTO>>> GetTenantDashboardChart(
            Guid tenantId,
            string chartType = "energyconsumption",
            string range = "30d",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            Guid? businessId = null)
        {
            var scope = await ResolveTenantScopeAsync(tenantId, businessId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetTenantDashboardChart(
                scope.TenantId, chartType, range, fromDate, toDate, scope.BusinessId));
        }

        [HttpGet("crm/tenant/suggestions")]
        public async Task<ActionResult<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>>> GetTenantDashboardSuggestions(
            Guid tenantId,
            Guid? businessId = null)
        {
            var scope = await ResolveTenantScopeAsync(tenantId, businessId);
            if (!scope.Allowed) return Forbid();
            return Ok(await energyDashboardService.GetTenantDashboardSuggestions(scope.TenantId, scope.BusinessId));
        }

        private async Task<UserAccessScope?> CurrentAccessAsync()
        {
            var currentUser = HttpContext.Items["User"] as UserResponseDTO;
            return currentUser is not null && Guid.TryParse(currentUser.userId, out var userId)
                ? await userAccessService.GetByUserIdAsync(userId, HttpContext.RequestAborted)
                : null;
        }

        private async Task<bool> CanAccessBusinessAsync(Guid businessId)
        {
            var access = await CurrentAccessAsync();
            // Preserve API-key-only internal callers; browser users are scoped by JWT.
            if (access is null) return true;
            if (!access.IsLoginAllowed || access.IsTenant) return false;
            return access.HasGlobalAccess || access.BusinessIds.Contains(businessId);
        }

        private async Task<(bool Allowed, Guid TenantId, Guid? BusinessId)> ResolveTenantScopeAsync(
            Guid requestedTenantId,
            Guid? requestedBusinessId)
        {
            var access = await CurrentAccessAsync();
            if (access is null || !access.IsLoginAllowed || !access.IsTenant || access.UserId != requestedTenantId)
                return (false, Guid.Empty, null);

            if (requestedBusinessId.HasValue && !access.BusinessIds.Contains(requestedBusinessId.Value))
                return (false, Guid.Empty, null);

            var businessId = requestedBusinessId ?? access.BusinessIds.FirstOrDefault();
            return (businessId == Guid.Empty ? false : true, access.UserId, businessId == Guid.Empty ? null : businessId);
        }

        private async Task<(bool Allowed, string? BusinessId, string? TenantId)> ResolveLegacyScopeAsync(
            string? requestedBusinessId,
            string? requestedTenantId)
        {
            var access = await CurrentAccessAsync();
            if (access is null)
                return (true, requestedBusinessId, requestedTenantId);

            if (!access.IsLoginAllowed) return (false, null, null);

            if (access.IsTenant)
            {
                Guid? businessId = null;
                if (Guid.TryParse(requestedBusinessId, out var parsedBusinessId))
                {
                    if (!access.BusinessIds.Contains(parsedBusinessId)) return (false, null, null);
                    businessId = parsedBusinessId;
                }
                else
                {
                    businessId = access.BusinessIds.FirstOrDefault();
                }

                return (businessId.HasValue && businessId.Value != Guid.Empty,
                    businessId?.ToString(), access.UserId.ToString());
            }

            if (Guid.TryParse(requestedBusinessId, out var requestedBusiness))
            {
                if (!access.HasGlobalAccess && !access.BusinessIds.Contains(requestedBusiness))
                    return (false, null, null);
                return (true, requestedBusiness.ToString(), null);
            }

            var firstBusiness = access.BusinessIds.FirstOrDefault();
            return (access.HasGlobalAccess || firstBusiness != Guid.Empty,
                firstBusiness == Guid.Empty ? requestedBusinessId : firstBusiness.ToString(), null);
        }
    }
}
