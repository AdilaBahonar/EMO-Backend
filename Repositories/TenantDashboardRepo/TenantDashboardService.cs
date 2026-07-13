using EMO.Repositories.DashboardServicesRepo;
using EMO.Repositories.UserAccessRepo;
using EnergyMonitor.DTOs;

namespace EMO.Repositories.TenantDashboardRepo;

public interface ITenantDashboardService
{
    Task<TenantDashboardDto?> GetRootAsync(Guid tenantId, DashboardQueryParams query);
}

public sealed class TenantDashboardService : ITenantDashboardService
{
    private readonly DashboardService _dashboard;
    private readonly IUserAccessService _access;

    public TenantDashboardService(DashboardService dashboard, IUserAccessService access)
    {
        _dashboard = dashboard;
        _access = access;
    }

    public async Task<TenantDashboardDto?> GetRootAsync(Guid tenantId, DashboardQueryParams query)
    {
        var scope = await _access.GetByUserIdAsync(tenantId);
        if (scope is null || !scope.IsLoginAllowed || !scope.IsTenant || scope.OfficeIds.Count == 0)
            return null;

        var officeDashboards = new List<OfficeDashboardDto>();
        foreach (var officeId in scope.OfficeIds)
        {
            var office = await _dashboard.GetOfficeAsync(officeId, query);
            if (office is not null) officeDashboards.Add(office);
        }

        var result = new TenantDashboardDto
        {
            BusinessId = scope.BusinessIds.FirstOrDefault(),
            BusinessName = "My assigned offices",
            Offices = officeDashboards.Select(x => new OfficeCardDto
            {
                OfficeId = x.OfficeId,
                OfficeName = x.OfficeName,
                TotalActiveEnergyKwh = x.Kpis.TotalActiveEnergyKwh,
                AvgPowerFactor = x.Kpis.AvgPowerFactor,
                SensorCount = x.Kpis.SensorCount,
                AlertCount = x.Kpis.AlertCount
            }).OrderBy(x => x.OfficeName).ToList()
        };

        var sensorCount = officeDashboards.Sum(x => x.Kpis.SensorCount);
        result.Kpis = new KpiSummaryDto
        {
            TotalActiveEnergyKwh = officeDashboards.Sum(x => x.Kpis.TotalActiveEnergyKwh),
            TotalReactiveEnergyKvarh = officeDashboards.Sum(x => x.Kpis.TotalReactiveEnergyKvarh),
            AvgActivePowerW = officeDashboards.Sum(x => x.Kpis.AvgActivePowerW),
            PeakActivePowerW = officeDashboards.Sum(x => x.Kpis.PeakActivePowerW),
            SensorCount = sensorCount,
            AlertCount = officeDashboards.Sum(x => x.Kpis.AlertCount),
            AvgPowerFactor = Weighted(officeDashboards, x => x.Kpis.AvgPowerFactor),
            AvgVoltage = Weighted(officeDashboards, x => x.Kpis.AvgVoltage),
            AvgCurrent = Weighted(officeDashboards, x => x.Kpis.AvgCurrent),
            AvgFrequency = Weighted(officeDashboards, x => x.Kpis.AvgFrequency)
        };

        result.HourlyEnergy = officeDashboards
            .SelectMany(x => x.HourlyEnergy)
            .GroupBy(x => x.Timestamp)
            .Select(x => new TimeSeriesPointDto { Timestamp = x.Key, Value = x.Sum(y => y.Value) })
            .OrderBy(x => x.Timestamp)
            .ToList();

        return result;
    }

    private static double Weighted(List<OfficeDashboardDto> rows, Func<OfficeDashboardDto, double> selector)
    {
        var denominator = rows.Sum(x => x.Kpis.SensorCount);
        return denominator == 0 ? 0 : rows.Sum(x => selector(x) * x.Kpis.SensorCount) / denominator;
    }
}
