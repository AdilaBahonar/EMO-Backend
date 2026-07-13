using EMO.Models.DBModels;
using EMO.Models.DTOs.SensorChainRedisDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace EMO.Repositories.SensorsChainRepo
{
    public class SensorRedisCacheService : ISensorRedisCacheService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redisDb;
        private readonly RedisKeys redisKeys;

        public SensorRedisCacheService(
            DBUserManagementContext db,
            IConnectionMultiplexer redis,
            IOptions<RedisKeys> redisKeysOptions)
        {
            this.db = db;
            redisDb = redis.GetDatabase();
            redisKeys = redisKeysOptions.Value;
        }

        public async Task SetSensorChainAsync(Guid sensorId)
        {
            var sensorChain = await db.tbl_sensor
                .Where(x => x.sensor_id == sensorId && !x.is_deleted)
                .Include(x => x.utility)
                .Include(x => x.device)
                    .ThenInclude(d => d.office)
                        .ThenInclude(o => o.section)
                            .ThenInclude(s => s.floor)
                                .ThenInclude(f => f.building)
                                    .ThenInclude(b => b.facility)
                                        .ThenInclude(fa => fa.business)
                .Select(x => new SensorChainRedisDTO
                {
                    SensorId = x.sensor_id,
                    SensorName = x.sensor_name,
                    MeterId = x.meter_id,
                    SerialAddress = x.serial_address,
                    StandbyAutoOff = x.standby_auto_off,

                    DeviceId = x.device.device_id,
                    DeviceName = x.device.device_name,
                    MacAddress = x.device.mac_address,

                    OfficeId = x.device.office.office_id,
                    OfficeName = x.device.office.office_name,
                    OfficeIsActive = x.device.office.is_active,
                    OfficeIsOccupied = x.device.office.is_occupied,
                    OfficeIs24Hours = x.device.office.is_24_hours,
                    OfficeAfterHoursAlertEnabled = x.device.office.after_hours_alert_enabled,
                    OfficeOpeningTime = x.device.office.opening_time,
                    OfficeClosingTime = x.device.office.closing_time,
                    OfficeWorkingDays = x.device.office.working_days,

                    SectionId = x.device.office.section.section_id,
                    SectionName = x.device.office.section.section_name,

                    FloorId = x.device.office.section.floor.floor_id,
                    FloorName = x.device.office.section.floor.floor_name,

                    BuildingId = x.device.office.section.floor.building.building_id,
                    BuildingName = x.device.office.section.floor.building.building_name,

                    FacilityId = x.device.office.section.floor.building.facility.facility_id,
                    FacilityName = x.device.office.section.floor.building.facility.facility_name,

                    BusinessId = x.device.office.section.floor.building.facility.business.business_id,
                    BusinessName = x.device.office.section.floor.building.facility.business.business_name,

                    UtilityId = x.fk_utility,
                    UtilityName = x.utility.utility_name,
                    CachedAtUtc = DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            if (sensorChain == null)
            {
                await DeleteSensorChainAsync(sensorId);
                return;
            }

            var activeAssignment = await db.tbl_sensor_appliance
                .Include(x => x.appliance)
                .Where(x => x.fk_sensor == sensorId && x.is_active && !x.is_deleted && !x.appliance.is_deleted)
                .OrderByDescending(x => x.assigned_at)
                .FirstOrDefaultAsync();

            if (activeAssignment?.appliance != null)
            {
                sensorChain.ApplianceId = activeAssignment.appliance.business_appliance_id;
                sensorChain.ApplianceName = activeAssignment.appliance.appliance_name;
                sensorChain.ApplianceCompanyName = activeAssignment.appliance.company_name;
                sensorChain.ApplianceModelNumber = activeAssignment.appliance.model_number;
                sensorChain.RatedVoltage = activeAssignment.appliance.rated_voltage;
                sensorChain.MinCurrent = activeAssignment.appliance.min_current;
                sensorChain.MaxCurrent = activeAssignment.appliance.max_current;
                sensorChain.MinPower = activeAssignment.appliance.min_power;
                sensorChain.MaxPower = activeAssignment.appliance.max_power;
                sensorChain.StandbyPower = activeAssignment.appliance.standby_power;
                sensorChain.NormalPowerFactor = activeAssignment.appliance.normal_power_factor;
                sensorChain.AppliancePriorityLevel = activeAssignment.appliance.priority_level;
                sensorChain.ApplianceIsCritical = activeAssignment.appliance.is_critical;
            }

            var loop = await db.tbl_hvac_loop_setting
                .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                .FirstOrDefaultAsync();

            if (loop != null)
            {
                sensorChain.HvacLoopSettingId = loop.hvac_loop_setting_id;
                sensorChain.HvacLoopEnabled = loop.loop_enabled;
                sensorChain.HvacLoopOnSeconds = loop.loop_on_seconds;
                sensorChain.HvacLoopOffSeconds = loop.loop_off_seconds;
                sensorChain.HvacLoopStartedAt = loop.loop_started_at;
            }

            var json = JsonSerializer.Serialize(sensorChain);
            await redisDb.StringSetAsync($"{redisKeys.SensorChainKeyPrefix}{sensorId}", json);

            await redisDb.SetAddAsync($"{redisKeys.BusinessSensorsKeyPrefix}{sensorChain.BusinessId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.FacilitySensorsKeyPrefix}{sensorChain.FacilityId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.BuildingSensorsKeyPrefix}{sensorChain.BuildingId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.FloorSensorsKeyPrefix}{sensorChain.FloorId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.SectionSensorsKeyPrefix}{sensorChain.SectionId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.OfficeSensorsKeyPrefix}{sensorChain.OfficeId}", sensorId.ToString());
            await redisDb.SetAddAsync($"{redisKeys.DeviceSensorsKeyPrefix}{sensorChain.DeviceId}", sensorId.ToString());
        }

        public async Task DeleteSensorChainAsync(Guid sensorId)
        {
            var key = $"{redisKeys.SensorChainKeyPrefix}{sensorId}";
            var json = await redisDb.StringGetAsync(key);

            await redisDb.KeyDeleteAsync(key);

            if (!json.HasValue) return;

            var sensorChain = JsonSerializer.Deserialize<SensorChainRedisDTO>(json!);
            if (sensorChain == null) return;

            await redisDb.SetRemoveAsync($"{redisKeys.BusinessSensorsKeyPrefix}{sensorChain.BusinessId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.FacilitySensorsKeyPrefix}{sensorChain.FacilityId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.BuildingSensorsKeyPrefix}{sensorChain.BuildingId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.FloorSensorsKeyPrefix}{sensorChain.FloorId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.SectionSensorsKeyPrefix}{sensorChain.SectionId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.OfficeSensorsKeyPrefix}{sensorChain.OfficeId}", sensorId.ToString());
            await redisDb.SetRemoveAsync($"{redisKeys.DeviceSensorsKeyPrefix}{sensorChain.DeviceId}", sensorId.ToString());
        }

        public async Task<bool> HasSensorCacheAsync()
        {
            var server = redisDb.Multiplexer.GetServer(redisDb.Multiplexer.GetEndPoints().First());
            return server.Keys(pattern: $"{redisKeys.SensorChainKeyPrefix}*").Any();
        }

        public async Task<bool> HasSensorChainAsync(Guid sensorId)
        {
            return await redisDb.KeyExistsAsync($"{redisKeys.SensorChainKeyPrefix}{sensorId}");
        }

        // Backward-compatible method. Existing callers can keep using this name.
        public async Task LoadAllSensorsChainAsync()
        {
            await RebuildAllSensorsChainAsync();
        }

        // Missing-only warmup. Useful if you do not want to overwrite existing Redis chain entries.
        public async Task EnsureAllSensorsChainAsync()
        {
            var sensorIds = await db.tbl_sensor
                .Where(x => !x.is_deleted)
                .Select(x => x.sensor_id)
                .ToListAsync();

            foreach (var sensorId in sensorIds)
            {
                if (!await HasSensorChainAsync(sensorId))
                    await SetSensorChainAsync(sensorId);
            }
        }

        // Recommended startup warmup. PostgreSQL is source of truth and Redis is rebuildable cache.
        public async Task RebuildAllSensorsChainAsync()
        {
            await DeleteKeysByPatternAsync($"{redisKeys.SensorChainKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.BusinessSensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.FacilitySensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.BuildingSensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.FloorSensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.SectionSensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.OfficeSensorsKeyPrefix}*");
            await DeleteKeysByPatternAsync($"{redisKeys.DeviceSensorsKeyPrefix}*");

            var sensorIds = await db.tbl_sensor
                .Where(x => !x.is_deleted)
                .Select(x => x.sensor_id)
                .ToListAsync();

            foreach (var sensorId in sensorIds)
                await SetSensorChainAsync(sensorId);
        }

        private async Task DeleteKeysByPatternAsync(string pattern)
        {
            var endpoints = redisDb.Multiplexer.GetEndPoints();
            if (endpoints.Length == 0) return;

            var server = redisDb.Multiplexer.GetServer(endpoints.First());
            foreach (var key in server.Keys(pattern: pattern).ToArray())
            {
                await redisDb.KeyDeleteAsync(key);
            }
        }
    }
}
