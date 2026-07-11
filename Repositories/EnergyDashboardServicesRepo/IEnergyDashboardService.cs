using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.EnergyDashboardServicesRepo
{
    public interface IEnergyDashboardService
    {
        Task<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>> GetMonthlyDeviceTypeReport(string? businessId = null, string? tenantId = null);
        Task<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>> GetEnergyConsumptionByDeviceTypeLast12Months(string? businessId = null, string? tenantId = null);
        Task<ResponseModel<PeakNonPeakSummaryResponseDTO>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null);
        Task<byte[]> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null);
        Task<byte[]> ExportEnergyConsumptionByDeviceTypeCsv(string? businessId = null, string? tenantId = null);

        Task<ResponseModel<CrmDashboardSummaryResponseDTO>> GetBusinessDashboardSummary(Guid businessId);
        Task<ResponseModel<CrmDashboardSummaryResponseDTO>> GetTenantDashboardSummary(Guid tenantId, Guid? businessId = null);
        Task<ResponseModel<CrmDashboardChartResponseDTO>> GetBusinessDashboardChart(Guid businessId, string chartType, string range = "30d", DateTime? fromDate = null, DateTime? toDate = null);
        Task<ResponseModel<CrmDashboardChartResponseDTO>> GetTenantDashboardChart(Guid tenantId, string chartType, string range = "30d", DateTime? fromDate = null, DateTime? toDate = null, Guid? businessId = null);
        Task<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>> GetBusinessDashboardSuggestions(Guid businessId);
        Task<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>> GetTenantDashboardSuggestions(Guid tenantId, Guid? businessId = null);
    }
}
