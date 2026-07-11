using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;

namespace EMO.Extensions.Seeders
{
    public static class DashboardRuntimeSeeder
    {
        public static async Task SeedDefaultDashboardSettingsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();

            var businessIds = await db.tbl_business
                .Where(x => !x.is_deleted)
                .Select(x => x.business_id)
                .ToListAsync();

            foreach (var businessId in businessIds)
            {
                var exists = await db.tbl_business_dashboard_setting
                    .AnyAsync(x => x.fk_business == businessId && !x.is_deleted);

                if (exists)
                {
                    continue;
                }

                db.tbl_business_dashboard_setting.Add(new tbl_business_dashboard_setting
                {
                    fk_business = businessId,
                    peak_start_time = "18:00",
                    peak_end_time = "23:00",
                    day_of_week = null,
                    timezone = "Asia/Karachi",
                    currency = "PKR",
                    tariff_rate = 65,
                    peak_tariff_rate = 75,
                    off_peak_tariff_rate = 55,
                    online_sensor_threshold_seconds = 60,
                    is_active = true,
                    is_deleted = false,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
