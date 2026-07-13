using EMO.Models.DBModels;
using EMO.Models.DTOs.RedisRuntimeDTOs;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace EMO.Repositories.DemandManagementRedisRepo
{
    public class DemandManagementRedisCacheService : IDemandManagementRedisCacheService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redis;
        private readonly string configKeyPrefix;
        private readonly string configuredBusinessesSetKey;
        private readonly string changedChannel;

        public DemandManagementRedisCacheService(
            DBUserManagementContext db,
            IConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            this.db = db;
            redis = connectionMultiplexer.GetDatabase();

            configKeyPrefix = configuration["RedisRuntimeKeys:DemandConfigKeyPrefix"]
                ?? "demand:config:";
            configuredBusinessesSetKey = configuration["RedisRuntimeKeys:DemandConfiguredBusinessesSetKey"]
                ?? "demand:config:businesses";
            changedChannel = configuration["RedisRuntimeKeys:DemandConfigChangedChannel"]
                ?? "demand:config:changed";
        }

        public async Task SetBusinessAsync(Guid businessId, string reason = "demand_configuration_saved")
        {
            var setting = await db.tbl_demand_management_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && !x.is_deleted)
                .Select(x => new DemandManagementRedisDTO
                {
                    DemandManagementSettingId = x.demand_management_setting_id,
                    BusinessId = x.fk_business,
                    DemandLimitKw = x.demand_limit_kw,
                    WarningThresholdPercent = x.warning_threshold_percent,
                    RecoveryThresholdKw = x.recovery_threshold_kw,
                    DemandIntervalMinutes = x.demand_interval_minutes,
                    StabilizationMinutes = x.stabilization_minutes,
                    EnablePeakHourControl = x.enable_peak_hour_control,
                    EnableDemandThresholdControl = x.enable_demand_threshold_control,
                    SuggestionOnlyMode = x.suggestion_only_mode,
                    IsActive = x.is_active,
                    UpdatedAtUtc = x.updated_at,
                    CachedAtUtc = DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            if (setting == null)
            {
                await DeleteBusinessAsync(businessId, "demand_configuration_missing");
                return;
            }

            var payload = JsonSerializer.Serialize(setting);
            await Task.WhenAll(
                redis.StringSetAsync($"{configKeyPrefix}{businessId}", payload),
                redis.SetAddAsync(configuredBusinessesSetKey, businessId.ToString())
            );

            await PublishChangedAsync(businessId, reason);
        }

        public async Task DeleteBusinessAsync(Guid businessId, string reason = "demand_configuration_deleted")
        {
            await Task.WhenAll(
                redis.KeyDeleteAsync($"{configKeyPrefix}{businessId}"),
                redis.SetRemoveAsync(configuredBusinessesSetKey, businessId.ToString())
            );

            await PublishChangedAsync(businessId, reason);
        }

        public async Task RebuildAllAsync()
        {
            await DeleteExistingConfigKeysAsync();
            await redis.KeyDeleteAsync(configuredBusinessesSetKey);

            var businessIds = await db.tbl_demand_management_setting
                .AsNoTracking()
                .Where(x => !x.is_deleted)
                .Select(x => x.fk_business)
                .Distinct()
                .ToListAsync();

            foreach (var businessId in businessIds)
            {
                await SetBusinessAsync(businessId, "startup_cache_rebuild");
            }
        }

        public async Task PublishChangedAsync(Guid businessId, string reason)
        {
            var payload = JsonSerializer.Serialize(new
            {
                businessId,
                reason,
                changedAtUtc = DateTime.UtcNow
            });

            await redis.PublishAsync(RedisChannel.Literal(changedChannel), payload);
        }

        private async Task DeleteExistingConfigKeysAsync()
        {
            var endpoints = redis.Multiplexer.GetEndPoints();
            if (endpoints.Length == 0) return;

            foreach (var endpoint in endpoints)
            {
                var server = redis.Multiplexer.GetServer(endpoint);
                if (!server.IsConnected) continue;

                foreach (var key in server.Keys(pattern: $"{configKeyPrefix}*").ToArray())
                {
                    await redis.KeyDeleteAsync(key);
                }
            }
        }
    }
}
