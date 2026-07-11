using EMO.Models.DBModels;
using EMO.Models.DTOs.RedisRuntimeDTOs;
using EMO.Repositories.SensorsChainRepo;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace EMO.Repositories.ApplianceRedisRepo
{
    public class ApplianceRedisCacheService : IApplianceRedisCacheService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redis;
        private readonly ISensorRedisCacheService sensorRedisCacheService;

        private const string ApplianceItemPrefix = "appliance:item:";
        private const string ApplianceBusinessPrefix = "appliances:business:";
        private const string ApplianceUtilityPrefix = "appliances:utility:";
        private const string ApplianceUtilityNamePrefix = "appliances:utility-name:";
        private const string ApplianceAllSetKey = "appliances:all";

        public ApplianceRedisCacheService(
            DBUserManagementContext db,
            IConnectionMultiplexer redis,
            ISensorRedisCacheService sensorRedisCacheService)
        {
            this.db = db;
            this.redis = redis.GetDatabase();
            this.sensorRedisCacheService = sensorRedisCacheService;
        }

        public async Task SetApplianceAsync(Guid applianceId)
        {
            var appliance = await db.tbl_business_appliance
                .Include(x => x.utility)
                .Where(x => x.business_appliance_id == applianceId && !x.is_deleted)
                .Select(x => new ApplianceRedisDTO
                {
                    ApplianceId = x.business_appliance_id,
                    BusinessId = x.fk_business,
                    DefaultApplianceId = x.fk_appliance,
                    ApplianceName = x.appliance_name,
                    CompanyName = x.company_name,
                    ModelNumber = x.model_number,
                    UtilityId = x.fk_utility,
                    UtilityName = x.utility.utility_name,
                    RatedVoltage = x.rated_voltage,
                    MinCurrent = x.min_current,
                    MaxCurrent = x.max_current,
                    MinPower = x.min_power,
                    MaxPower = x.max_power,
                    StandbyPower = x.standby_power,
                    NormalPowerFactor = x.normal_power_factor,
                    IsShiftable = x.is_shiftable,
                    PriorityLevel = x.priority_level,
                    NormalOperatingHours = x.normal_operating_hours,
                    CanAutoControl = x.can_auto_control,
                    IsCritical = x.is_critical,
                    AllowOptimizationSuggestions = x.allow_optimization_suggestions,
                    AllowedShiftStartTime = x.allowed_shift_start_time,
                    AllowedShiftEndTime = x.allowed_shift_end_time,
                    MinimumOnDurationMinutes = x.minimum_on_duration_minutes,
                    MinimumOffDurationMinutes = x.minimum_off_duration_minutes,
                    IsDefault = x.is_default,
                    IsCustom = x.is_custom,
                    IsActive = x.is_active,
                    CachedAtUtc = DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            if (appliance == null)
            {
                await DeleteApplianceAsync(applianceId);
                return;
            }

            var json = JsonSerializer.Serialize(appliance);
            var id = appliance.ApplianceId.ToString();

            // Use awaited Redis calls instead of fire-and-forget batch tasks.
            // This guarantees Redis is updated before the API returns success.
            await redis.StringSetAsync($"{ApplianceItemPrefix}{id}", json);
            await redis.SetAddAsync(ApplianceAllSetKey, id);
            await redis.SetAddAsync($"{ApplianceBusinessPrefix}{appliance.BusinessId}", id);
            await redis.SetAddAsync($"{ApplianceUtilityPrefix}{appliance.UtilityId}", id);
            await redis.SetAddAsync($"{ApplianceUtilityNamePrefix}{Normalize(appliance.UtilityName)}", id);
        }

        public async Task DeleteApplianceAsync(Guid applianceId)
        {
            var id = applianceId.ToString();
            var key = $"{ApplianceItemPrefix}{id}";
            var json = await redis.StringGetAsync(key);

            ApplianceRedisDTO? dto = null;

            if (json.HasValue)
            {
                dto = JsonSerializer.Deserialize<ApplianceRedisDTO>(json!);
            }

            // Fallback: if item key is already missing but the ID is still in utility sets,
            // use DB to remove it from those sets as well.
            if (dto == null)
            {
                dto = await db.tbl_business_appliance
                    .Include(x => x.utility)
                    .Where(x => x.business_appliance_id == applianceId)
                    .Select(x => new ApplianceRedisDTO
                    {
                        ApplianceId = x.business_appliance_id,
                        BusinessId = x.fk_business,
                        UtilityId = x.fk_utility,
                        UtilityName = x.utility.utility_name
                    })
                    .FirstOrDefaultAsync();
            }

            if (dto != null)
            {
                await redis.SetRemoveAsync($"{ApplianceBusinessPrefix}{dto.BusinessId}", id);
                await redis.SetRemoveAsync($"{ApplianceUtilityPrefix}{dto.UtilityId}", id);
                await redis.SetRemoveAsync($"{ApplianceUtilityNamePrefix}{Normalize(dto.UtilityName)}", id);
            }

            await redis.KeyDeleteAsync(key);
            await redis.SetRemoveAsync(ApplianceAllSetKey, id);
        }

        public async Task RebuildAllAppliancesAsync()
        {
            await DeleteKeysByPatternAsync("appliance:item:*");
            await DeleteKeysByPatternAsync("appliances:business:*");
            await DeleteKeysByPatternAsync("appliances:utility:*");
            await DeleteKeysByPatternAsync("appliances:utility-name:*");
            await redis.KeyDeleteAsync(ApplianceAllSetKey);

            var applianceIds = await db.tbl_business_appliance
                .Where(x => !x.is_deleted)
                .Select(x => x.business_appliance_id)
                .ToListAsync();

            foreach (var applianceId in applianceIds)
                await SetApplianceAsync(applianceId);
        }

        public async Task RefreshSensorChainsForApplianceAsync(Guid applianceId)
        {
            var sensorIds = await db.tbl_sensor_appliance
                .Where(x => x.fk_appliance == applianceId && !x.is_deleted)
                .Select(x => x.fk_sensor)
                .Distinct()
                .ToListAsync();

            foreach (var sensorId in sensorIds)
                await sensorRedisCacheService.SetSensorChainAsync(sensorId);
        }

        public async Task<bool> HasApplianceCacheAsync()
        {
            return await redis.KeyExistsAsync(ApplianceAllSetKey);
        }

        private async Task DeleteKeysByPatternAsync(string pattern)
        {
            var endpoints = redis.Multiplexer.GetEndPoints();
            if (endpoints.Length == 0) return;

            var server = redis.Multiplexer.GetServer(endpoints.First());
            foreach (var key in server.Keys(pattern: pattern).ToArray())
            {
                await redis.KeyDeleteAsync(key);
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "unknown"
                : value.Trim().ToLowerInvariant().Replace(" ", "-");
        }
    }
}
