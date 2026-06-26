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
        public async Task<ActionResult<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>>> GetMonthlyDeviceTypeReport()
        {
            var response = await energyDashboardService.GetMonthlyDeviceTypeReport();
            return Ok(response);
        }

        [HttpGet("GetEnergyConsumptionByDeviceTypeLast12Months")]
        public async Task<ActionResult<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>>> GetEnergyConsumptionByDeviceTypeLast12Months()
        {
            var response = await energyDashboardService.GetEnergyConsumptionByDeviceTypeLast12Months();
            return Ok(response);
        }

        [HttpGet("GetPeakNonPeakAnalysis")]
        public async Task<ActionResult<ResponseModel<PeakNonPeakSummaryResponseDTO>>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate)
        {
            var response = await energyDashboardService.GetPeakNonPeakAnalysis(startDate, endDate);
            return Ok(response);
        }

        [HttpGet("ExportPeakNonPeakAnalysisCsv")]
        public async Task<IActionResult> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate)
        {
            var fileBytes = await energyDashboardService.ExportPeakNonPeakAnalysisCsv(startDate, endDate);
            return File(fileBytes, "text/csv", "peak-non-peak-analysis.csv");
        }

        [HttpGet("ExportEnergyConsumptionByDeviceTypeCsv")]
        public async Task<IActionResult> ExportEnergyConsumptionByDeviceTypeCsv()
        {
            var fileBytes = await energyDashboardService.ExportEnergyConsumptionByDeviceTypeCsv();
            return File(fileBytes, "text/csv", "energy-consumption-by-device-type.csv");
        }
    }
}