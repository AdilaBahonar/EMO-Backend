using EMO.Models.DBModels;
using EMO.Repositories.DeepDiveRepo;
using EMO.Repositories.EnergyDashboardServicesRepo;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace EMO.Repositories.DashboardServicesRepo
{
    /// <summary>
    /// Refreshes the compact sensor-energy layer first, then rebuilds the most useful
    /// dashboard snapshots. The PostgreSQL advisory lock in the aggregate store keeps
    /// multiple API instances from running the same heavy cycle simultaneously.
    /// </summary>
    public class DashboardAggregateWorker : BackgroundService
    {
        private const long WorkerAdvisoryLockKey = 704_220_260;
        private static readonly string[] StandardBusinessRanges = { "24h", "7d", "30d" };
        private static readonly string[] HierarchyRanges = { "24h", "7d", "30d" };

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<DashboardAggregateWorker> logger;
        private bool firstRun = true;

        public DashboardAggregateWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DashboardAggregateWorker> logger)
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
                    firstRun = false;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Dashboard aggregate worker failed. It will retry on the next interval.");
                }

                var now = DateTime.UtcNow;
                var nextRefresh = new DateTime(
                    now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc)
                    .AddHours(1)
                    .AddMinutes(6);
                await Task.Delay(nextRefresh - now, stoppingToken);
            }
        }

        private async Task WarmDashboardAggregatesAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            var connection = db.Database.GetDbConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException(
                    "Dashboard aggregation requires the PostgreSQL/Npgsql provider.");

            var openedHere = connection.State != ConnectionState.Open;
            if (openedHere)
                await connection.OpenAsync(stoppingToken);

            var workerLockAcquired = false;
            try
            {
                await using (var lockCommand = new NpgsqlCommand(
                    "SELECT pg_try_advisory_lock(@lock_key);", connection))
                {
                    lockCommand.Parameters.AddWithValue("lock_key", WorkerAdvisoryLockKey);
                    workerLockAcquired = Convert.ToBoolean(
                        await lockCommand.ExecuteScalarAsync(stoppingToken) ?? false);
                }

                if (!workerLockAcquired)
                {
                    logger.LogInformation(
                        "Dashboard aggregate cycle skipped because another API instance is running it.");
                    return;
                }

                var energyAggregateStore = scope.ServiceProvider
                    .GetRequiredService<ISensorEnergyAggregateStore>();
                await energyAggregateStore.RefreshAsync(stoppingToken);

                var dashboard = scope.ServiceProvider.GetRequiredService<IEnergyDashboardService>();
                var deepDive = scope.ServiceProvider.GetRequiredService<IDeepDiveService>();

                await WarmDeepDiveAggregatesAsync(db, deepDive, stoppingToken);
                db.ChangeTracker.Clear();
                await WarmCrmAggregatesAsync(db, dashboard, stoppingToken);
            }
            finally
            {
                if (workerLockAcquired)
                {
                    try
                    {
                        await using var unlockCommand = new NpgsqlCommand(
                            "SELECT pg_advisory_unlock(@lock_key);", connection);
                        unlockCommand.Parameters.AddWithValue("lock_key", WorkerAdvisoryLockKey);
                        await unlockCommand.ExecuteScalarAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Could not release the dashboard worker advisory lock cleanly.");
                    }
                }

                if (openedHere)
                    await connection.CloseAsync();
            }
        }

        private static async Task WarmCrmAggregatesAsync(
            DBUserManagementContext db,
            IEnergyDashboardService dashboard,
            CancellationToken stoppingToken)
        {
            var businessIds = await db.tbl_business
                .AsNoTracking()
                .Where(x => !x.is_deleted)
                .Select(x => x.business_id)
                .ToListAsync(stoppingToken);

            foreach (var businessId in businessIds)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await dashboard.GetBusinessDashboardSummary(businessId);
                await dashboard.GetBusinessDashboardSuggestions(businessId);
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
                await dashboard.GetTenantDashboardSuggestions(tenant.fk_tenant, tenant.fk_business);
            }
        }

        private async Task WarmDeepDiveAggregatesAsync(
            DBUserManagementContext db,
            IDeepDiveService deepDive,
            CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var businessRanges = StandardBusinessRanges.ToList();

            // 90-day snapshots do not need hourly rebuilding.
            if (firstRun || now.Hour % 6 == 0)
                businessRanges.Add("90d");

            // The one-year snapshot is rebuilt on startup and once per day.
            if (firstRun || now.Hour == 2)
                businessRanges.Add("1y");

            var businesses = (await db.tbl_business.AsNoTracking()
                .Where(x => !x.is_deleted)
                .Select(x => x.business_id)
                .ToListAsync(stoppingToken))
                .Select(id => (Level: "business", Id: id));

            await WarmScopesAsync(deepDive, businesses, businessRanges, stoppingToken);

            // These hierarchy levels are common drill-down entry points. They use
            // compact 15-minute aggregates, so 90d/1y can be calculated quickly on
            // demand without pre-building every possible scope every hour.
            var hierarchyScopes = new List<(string Level, Guid Id)>();
            hierarchyScopes.AddRange((await db.tbl_facility.AsNoTracking()
                .Where(x => !x.is_deleted).Select(x => x.facility_id)
                .ToListAsync(stoppingToken)).Select(id => ("facility", id)));
            hierarchyScopes.AddRange((await db.tbl_building.AsNoTracking()
                .Where(x => !x.is_deleted).Select(x => x.building_id)
                .ToListAsync(stoppingToken)).Select(id => ("building", id)));
            hierarchyScopes.AddRange((await db.tbl_floor.AsNoTracking()
                .Where(x => !x.is_deleted).Select(x => x.floor_id)
                .ToListAsync(stoppingToken)).Select(id => ("floor", id)));

            await WarmScopesAsync(deepDive, hierarchyScopes, HierarchyRanges, stoppingToken);
        }

        private async Task WarmScopesAsync(
            IDeepDiveService deepDive,
            IEnumerable<(string Level, Guid Id)> scopes,
            IEnumerable<string> ranges,
            CancellationToken stoppingToken)
        {
            foreach (var range in ranges)
            {
                foreach (var scope in scopes)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        await deepDive.WarmScopeAsync(
                            scope.Level, scope.Id, range, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Deep Dive aggregate failed for {Level}/{Id} range {Range}.",
                            scope.Level, scope.Id, range);
                    }
                }
            }
        }
    }
}
