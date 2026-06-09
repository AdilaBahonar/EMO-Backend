using EMO.Models.DBModels;
using EMO.Models.DTOs.SensorChainRedisDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
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

                    DeviceId = x.device.device_id,
                    DeviceName = x.device.device_name,

                    OfficeId = x.device.office.office_id,
                    OfficeName = x.device.office.office_name,

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
                    UtilityName = x.utility.utility_name
                })
                .FirstOrDefaultAsync();

            if (sensorChain == null)
                return;

            var json = JsonSerializer.Serialize(sensorChain);

            await redisDb.StringSetAsync(
                $"{redisKeys.SensorChainKeyPrefix}{sensorId}",
                json);

            await redisDb.SetAddAsync(
                $"{redisKeys.BusinessSensorsKeyPrefix}{sensorChain.BusinessId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.FacilitySensorsKeyPrefix}{sensorChain.FacilityId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.BuildingSensorsKeyPrefix}{sensorChain.BuildingId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.FloorSensorsKeyPrefix}{sensorChain.FloorId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.SectionSensorsKeyPrefix}{sensorChain.SectionId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.OfficeSensorsKeyPrefix}{sensorChain.OfficeId}",
                sensorId.ToString());

            await redisDb.SetAddAsync(
                $"{redisKeys.DeviceSensorsKeyPrefix}{sensorChain.DeviceId}",
                sensorId.ToString());
        }

        public async Task DeleteSensorChainAsync(Guid sensorId)
        {
            var key = $"{redisKeys.SensorChainKeyPrefix}{sensorId}";
            var json = await redisDb.StringGetAsync(key);

            if (!json.HasValue)
            {
                await redisDb.KeyDeleteAsync(key);
                return;
            }

            var sensorChain = JsonSerializer.Deserialize<SensorChainRedisDTO>(json!);

            await redisDb.KeyDeleteAsync(key);

            if (sensorChain == null)
                return;

            await redisDb.SetRemoveAsync(
                $"{redisKeys.BusinessSensorsKeyPrefix}{sensorChain.BusinessId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.FacilitySensorsKeyPrefix}{sensorChain.FacilityId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.BuildingSensorsKeyPrefix}{sensorChain.BuildingId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.FloorSensorsKeyPrefix}{sensorChain.FloorId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.SectionSensorsKeyPrefix}{sensorChain.SectionId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.OfficeSensorsKeyPrefix}{sensorChain.OfficeId}",
                sensorId.ToString());

            await redisDb.SetRemoveAsync(
                $"{redisKeys.DeviceSensorsKeyPrefix}{sensorChain.DeviceId}",
                sensorId.ToString());
        }

        public async Task<bool> HasSensorCacheAsync()
        {
            var server = redisDb.Multiplexer.GetServer(
                redisDb.Multiplexer.GetEndPoints().First());

            var pattern = $"{redisKeys.SensorChainKeyPrefix}*";

            return server.Keys(pattern: pattern).Any();
        }

        public async Task LoadAllSensorsChainAsync()
        {
            var sensorIds = await db.tbl_sensor
                .Where(x => !x.is_deleted)
                .Select(x => x.sensor_id)
                .ToListAsync();

            foreach (var sensorId in sensorIds)
            {
                await SetSensorChainAsync(sensorId);
            }
        }
    }
}
