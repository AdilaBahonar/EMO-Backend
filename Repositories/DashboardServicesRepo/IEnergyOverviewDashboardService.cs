using EMO.Models.DTOs.DashboardDTOs;
using EnergyMonitor.DTOs;

namespace EMO.Repositories.DashboardServicesRepo
{
    public interface IEnergyOverviewDashboardService
    {
        //    Task<EnergyOverviewDashboardDto> GetBusinessOverviewAsync(
        //    Guid businessId,
        //    DashboardQueryParams q
        //);

        Task<EnergyOverviewDashboardDto> GetBusinessOverviewAsync(
            Guid businessId,
            DashboardQueryParams q);

        Task<EnergyOverviewDashboardDto> GetOverviewAsync(
            string level,
            Guid id,
            DashboardQueryParams q);
    }
}
