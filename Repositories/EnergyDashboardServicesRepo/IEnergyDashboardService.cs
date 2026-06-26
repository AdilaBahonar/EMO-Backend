using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.EnergyDashboardServicesRepo
{
    public interface IEnergyDashboardService
    {
        Task<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>> GetMonthlyDeviceTypeReport();
        Task<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>> GetEnergyConsumptionByDeviceTypeLast12Months();
        Task<ResponseModel<PeakNonPeakSummaryResponseDTO>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportEnergyConsumptionByDeviceTypeCsv();
    }
}