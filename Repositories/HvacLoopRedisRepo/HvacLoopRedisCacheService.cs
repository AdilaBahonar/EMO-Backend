using EMO.Models.DBModels;
using EMO.Models.DTOs.RedisRuntimeDTOs;
using EMO.Repositories.SensorsChainRepo;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace EMO.Repositories.HvacLoopRedisRepo
{
    public class HvacLoopRedisCacheService : IHvacLoopRedisCacheService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redis;
        private readonly ISensorRedisCacheService sensorRedisCacheService;

        private const string HvacLoopKeyPrefix = "hvac:loop:";
        private const string HvacLoopEnabledSetKey = "hvac:loop:enabled";
        private const string HvacLoopChangedChannel = "hvac:loop:changed";
        private const string HvacLoopLastStatePrefix = "hvac:loop:last-state:";

        public HvacLoopRedisCacheService(
            DBUserManagementContext db,
            IConnectionMultiplexer redis,
            ISensorRedisCacheService sensorRedisCacheService)
        {
            this.db = db;
            this.redis = redis.GetDatabase();
            this.sensorRedisCacheService = sensorRedisCacheService;
        }

        public async Task SetLoopSettingAsync(Guid sensorId, string reason = "loop_setting_saved")
        {
            var setting = await db.tbl_hvac_loop_setting
                .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                .Select(x => new HvacLoopRedisDTO
                {
                    HvacLoopSettingId = x.hvac_loop_setting_id,
                    SensorId = x.fk_sensor,
                    SensorName = x.sensor.sensor_name,
                    MacAddress = x.sensor.device.mac_address,
                    SerialAddress = x.sensor.serial_address,
                    LoopEnabled = x.loop_enabled,
                    LoopOnSeconds = x.loop_on_seconds,
                    LoopOffSeconds = x.loop_off_seconds,
                    LoopStartedAtUtc = x.loop_started_at,
                    IsActive = x.is_active,
                    CachedAtUtc = DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            if (setting == null)
            {
                await DeleteLoopSettingAsync(sensorId);
                return;
            }

            var key = $"{HvacLoopKeyPrefix}{sensorId}";
            await redis.StringSetAsync(key, JsonSerializer.Serialize(setting));

            if (setting.LoopEnabled && setting.IsActive && setting.LoopOnSeconds > 0 && setting.LoopOffSeconds > 0)
                await redis.SetAddAsync(HvacLoopEnabledSetKey, sensorId.ToString());
            else
                await redis.SetRemoveAsync(HvacLoopEnabledSetKey, sensorId.ToString());

            await sensorRedisCacheService.SetSensorChainAsync(sensorId);
            await PublishLoopChangedAsync(sensorId, reason);
        }

        public async Task DeleteLoopSettingAsync(Guid sensorId)
        {
            await redis.KeyDeleteAsync($"{HvacLoopKeyPrefix}{sensorId}");
            await redis.SetRemoveAsync(HvacLoopEnabledSetKey, sensorId.ToString());
            await redis.KeyDeleteAsync($"{HvacLoopLastStatePrefix}{sensorId}");
            await sensorRedisCacheService.SetSensorChainAsync(sensorId);
            await PublishLoopChangedAsync(sensorId, "loop_setting_deleted");
        }

        public async Task RebuildAllLoopSettingsAsync()
        {
            await redis.KeyDeleteAsync(HvacLoopEnabledSetKey);
            await DeleteKeysByPatternAsync($"{HvacLoopKeyPrefix}*");

            var sensorIds = await db.tbl_hvac_loop_setting
                .Where(x => !x.is_deleted)
                .Select(x => x.fk_sensor)
                .Distinct()
                .ToListAsync();

            foreach (var sensorId in sensorIds)
                await SetLoopSettingAsync(sensorId, "startup_cache_rebuild");
        }

        public async Task PublishLoopChangedAsync(Guid sensorId, string reason)
        {
            var payload = JsonSerializer.Serialize(new
            {
                sensorId,
                reason,
                changedAtUtc = DateTime.UtcNow
            });

            await redis.PublishAsync(RedisChannel.Literal(HvacLoopChangedChannel), payload);
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
    }
}
