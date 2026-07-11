using EMO.Models.DBModels;
using EMO.Repositories.EnergyDashboardServicesRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.DashboardServicesRepo
{
    /// <summary>
    /// Warms CRM dashboard aggregates. PostgreSQL remains the source of truth;
    /// Redis is only a short-lived cache used by EnergyDashboardService.
    /// </summary>
    public class DashboardAggregateWorker : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<DashboardAggregateWorker> logger;

        public DashboardAggregateWorker(IServiceScopeFactory scopeFactory, ILogger<DashboardAggregateWorker> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WarmDashboardAggregatesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Dashboard aggregate worker failed. It will retry on the next hourly interval.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task WarmDashboardAggregatesAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            var dashboard = scope.ServiceProvider.GetRequiredService<IEnergyDashboardService>();

            var businessIds = await db.tbl_business
                .AsNoTracking()
                .Where(x => !x.is_deleted)
                .Select(x => x.business_id)
                .ToListAsync(stoppingToken);

            foreach (var businessId in businessIds)
            {
                stoppingToken.ThrowIfCancellationRequested();

                await dashboard.GetBusinessDashboardSummary(businessId);                 // fixed CRM KPI/default = 30 days
                await dashboard.GetMonthlyDeviceTypeReport(businessId.ToString());        // frontend legacy donut endpoint
                await dashboard.GetEnergyConsumptionByDeviceTypeLast12Months(businessId.ToString()); // frontend legacy trend endpoint
                await dashboard.GetBusinessDashboardChart(businessId, "energyconsumption", "24h");
                await dashboard.GetBusinessDashboardChart(businessId, "energyconsumption", "7d");
                await dashboard.GetBusinessDashboardChart(businessId, "energyconsumption", "30d");
                await dashboard.GetBusinessDashboardChart(businessId, "energyconsumption", "1y");
                await dashboard.GetBusinessDashboardChart(businessId, "peaknonpeak", "24h");
                await dashboard.GetBusinessDashboardChart(businessId, "peaknonpeak", "7d");
                await dashboard.GetBusinessDashboardChart(businessId, "peaknonpeak", "30d");
                await dashboard.GetBusinessDashboardChart(businessId, "peaknonpeak", "1y");
                await dashboard.GetBusinessDashboardChart(businessId, "peakdemand", "24h");
                await dashboard.GetBusinessDashboardChart(businessId, "peakdemand", "7d");
                await dashboard.GetBusinessDashboardChart(businessId, "peakdemand", "30d");
                await dashboard.GetBusinessDashboardChart(businessId, "peakdemand", "1y");
                await dashboard.GetBusinessDashboardChart(businessId, "highdemand", "1y");
                await dashboard.GetBusinessDashboardChart(businessId, "hourlyusage", "24h");
                await dashboard.GetBusinessDashboardChart(businessId, "utilitywise", "30d");
                await dashboard.GetBusinessDashboardSuggestions(businessId);             // fixed optimization window = 7 days
            }

            var tenants = await db.tbl_agreement
                .AsNoTracking()
                .Where(x => !x.is_deleted && x.is_active)
                .Select(x => new { x.fk_tenant, x.fk_business })
                .Distinct()
                .ToListAsync(stoppingToken);

            foreach (var tenant in tenants)
            {
                stoppingToken.ThrowIfCancellationRequested();

                await dashboard.GetTenantDashboardSummary(tenant.fk_tenant, tenant.fk_business);
                await dashboard.GetMonthlyDeviceTypeReport(tenant.fk_business.ToString(), tenant.fk_tenant.ToString()); // frontend legacy donut endpoint
                await dashboard.GetEnergyConsumptionByDeviceTypeLast12Months(tenant.fk_business.ToString(), tenant.fk_tenant.ToString()); // frontend legacy trend endpoint
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "energyconsumption", "24h", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "energyconsumption", "7d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "energyconsumption", "30d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "energyconsumption", "1y", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peaknonpeak", "24h", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peaknonpeak", "7d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peaknonpeak", "30d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peaknonpeak", "1y", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peakdemand", "24h", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peakdemand", "7d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peakdemand", "30d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "peakdemand", "1y", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "highdemand", "1y", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "hourlyusage", "24h", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardChart(tenant.fk_tenant, "utilitywise", "30d", businessId: tenant.fk_business);
                await dashboard.GetTenantDashboardSuggestions(tenant.fk_tenant, tenant.fk_business);
            }
        }
    }
}
