//using EMO.Extensions.MiddleWare;
//using EMO.Models.DTOs.EnergyDashboardDTOs;
//using EMO.Models.DTOs.ResponseDTO;
//using EMO.Repositories.EnergyDashboardServicesRepo;
//using Microsoft.AspNetCore.Mvc;

//namespace EMO.Controllers
//{
//    [ApiKey]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EnergyDashboardController : ControllerBase
//    {
//        private readonly IEnergyDashboardService energyDashboardService;

//        public EnergyDashboardController(IEnergyDashboardService energyDashboardService)
//        {
//            this.energyDashboardService = energyDashboardService;
//        }

//        [HttpGet("GetMonthlyDeviceTypeReport")]
//        public async Task<ActionResult<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>>> GetMonthlyDeviceTypeReport()
//        {
//            var response = await energyDashboardService.GetMonthlyDeviceTypeReport();
//            return Ok(response);
//        }

//        [HttpGet("GetEnergyConsumptionByDeviceTypeLast12Months")]
//        public async Task<ActionResult<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>>> GetEnergyConsumptionByDeviceTypeLast12Months()
//        {
//            var response = await energyDashboardService.GetEnergyConsumptionByDeviceTypeLast12Months();
//            return Ok(response);
//        }

//        [HttpGet("GetPeakNonPeakAnalysis")]
//        public async Task<ActionResult<ResponseModel<PeakNonPeakSummaryResponseDTO>>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate)
//        {
//            var response = await energyDashboardService.GetPeakNonPeakAnalysis(startDate, endDate);
//            return Ok(response);
//        }

//        [HttpGet("ExportPeakNonPeakAnalysisCsv")]
//        public async Task<IActionResult> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate)
//        {
//            var fileBytes = await energyDashboardService.ExportPeakNonPeakAnalysisCsv(startDate, endDate);
//            return File(fileBytes, "text/csv", "peak-non-peak-analysis.csv");
//        }

//        [HttpGet("ExportEnergyConsumptionByDeviceTypeCsv")]
//        public async Task<IActionResult> ExportEnergyConsumptionByDeviceTypeCsv()
//        {
//            var fileBytes = await energyDashboardService.ExportEnergyConsumptionByDeviceTypeCsv();
//            return File(fileBytes, "text/csv", "energy-consumption-by-device-type.csv");
//        }
//    }
//}


using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.EnergyDashboardServicesRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyDashboardController : ControllerBase
    {
        private readonly IEnergyDashboardService energyDashboardService;

        public EnergyDashboardController(IEnergyDashboardService energyDashboardService)
        {
            this.energyDashboardService = energyDashboardService;
        }

        [HttpGet("GetMonthlyDeviceTypeReport")]
        public async Task<ActionResult<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>>> GetMonthlyDeviceTypeReport(string? businessId = null, string? tenantId = null)
        {
            var response = await energyDashboardService.GetMonthlyDeviceTypeReport(businessId, tenantId);
            return Ok(response);
        }

        [HttpGet("GetEnergyConsumptionByDeviceTypeLast12Months")]
        public async Task<ActionResult<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>>> GetEnergyConsumptionByDeviceTypeLast12Months(string? businessId = null, string? tenantId = null)
        {
            var response = await energyDashboardService.GetEnergyConsumptionByDeviceTypeLast12Months(businessId, tenantId);
            return Ok(response);
        }

        [HttpGet("GetPeakNonPeakAnalysis")]
        public async Task<ActionResult<ResponseModel<PeakNonPeakSummaryResponseDTO>>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null)
        {
            var response = await energyDashboardService.GetPeakNonPeakAnalysis(startDate, endDate, businessId, tenantId);
            return Ok(response);
        }

        [HttpGet("ExportPeakNonPeakAnalysisCsv")]
        public async Task<IActionResult> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null)
        {
            var fileBytes = await energyDashboardService.ExportPeakNonPeakAnalysisCsv(startDate, endDate, businessId, tenantId);
            return File(fileBytes, "text/csv", "peak-non-peak-analysis.csv");
        }

        [HttpGet("ExportEnergyConsumptionByDeviceTypeCsv")]
        public async Task<IActionResult> ExportEnergyConsumptionByDeviceTypeCsv(string? businessId = null, string? tenantId = null)
        {
            var fileBytes = await energyDashboardService.ExportEnergyConsumptionByDeviceTypeCsv(businessId, tenantId);
            return File(fileBytes, "text/csv", "energy-consumption-by-device-type.csv");
        }

        [HttpGet("crm/business/summary")]
        public async Task<ActionResult<ResponseModel<CrmDashboardSummaryResponseDTO>>> GetBusinessDashboardSummary(Guid businessId)
        {
            var response = await energyDashboardService.GetBusinessDashboardSummary(businessId);
            return Ok(response);
        }

        [HttpGet("crm/business/live-overview")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ResponseModel<CrmDashboardLiveOverviewResponseDTO>>> GetBusinessDashboardLiveOverview(Guid businessId, bool forceRefresh = false)
        {
            var response = await energyDashboardService.GetBusinessDashboardLiveOverview(businessId, forceRefresh);
            return Ok(response);
        }

        [HttpGet("crm/business/chart")]
        public async Task<ActionResult<ResponseModel<CrmDashboardChartResponseDTO>>> GetBusinessDashboardChart(Guid businessId, string chartType = "energyconsumption", string range = "30d", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var response = await energyDashboardService.GetBusinessDashboardChart(businessId, chartType, range, fromDate, toDate);
            return Ok(response);
        }

        [HttpGet("crm/business/suggestions")]
        public async Task<ActionResult<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>>> GetBusinessDashboardSuggestions(Guid businessId)
        {
            var response = await energyDashboardService.GetBusinessDashboardSuggestions(businessId);
            return Ok(response);
        }

        [HttpGet("crm/tenant/summary")]
        public async Task<ActionResult<ResponseModel<CrmDashboardSummaryResponseDTO>>> GetTenantDashboardSummary(Guid tenantId, Guid? businessId = null)
        {
            var response = await energyDashboardService.GetTenantDashboardSummary(tenantId, businessId);
            return Ok(response);
        }

        [HttpGet("crm/tenant/live-overview")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<ResponseModel<CrmDashboardLiveOverviewResponseDTO>>> GetTenantDashboardLiveOverview(Guid tenantId, Guid? businessId = null, bool forceRefresh = false)
        {
            var response = await energyDashboardService.GetTenantDashboardLiveOverview(tenantId, businessId, forceRefresh);
            return Ok(response);
        }

        [HttpGet("crm/tenant/chart")]
        public async Task<ActionResult<ResponseModel<CrmDashboardChartResponseDTO>>> GetTenantDashboardChart(Guid tenantId, string chartType = "energyconsumption", string range = "30d", DateTime? fromDate = null, DateTime? toDate = null, Guid? businessId = null)
        {
            var response = await energyDashboardService.GetTenantDashboardChart(tenantId, chartType, range, fromDate, toDate, businessId);
            return Ok(response);
        }

        [HttpGet("crm/tenant/suggestions")]
        public async Task<ActionResult<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>>> GetTenantDashboardSuggestions(Guid tenantId, Guid? businessId = null)
        {
            var response = await energyDashboardService.GetTenantDashboardSuggestions(tenantId, businessId);
            return Ok(response);
        }
    }
}
