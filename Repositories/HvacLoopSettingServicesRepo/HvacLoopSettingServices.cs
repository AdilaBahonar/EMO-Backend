using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.HvacLoopSettingDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.HvacLoopRedisRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.HvacLoopSettingServicesRepo
{
    public class HvacLoopSettingServices : IHvacLoopSettingServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly IHvacLoopRedisCacheService hvacLoopRedisCacheService;


        public HvacLoopSettingServices(DBUserManagementContext db, IMapper mapper, IHvacLoopRedisCacheService hvacLoopRedisCacheService)
        {
            this.db = db;
            this.mapper = mapper;
            this.hvacLoopRedisCacheService = hvacLoopRedisCacheService;
        }

        private static bool IsValidGuid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);
        }

        private static bool IsHvacSensor(tbl_sensor sensor)
        {
            return sensor.utility != null &&
                   sensor.utility.utility_name.Equals("HVAC", StringComparison.OrdinalIgnoreCase);
        }

        private static string ValidateLoopTiming(bool loopEnabled, int loopOnSeconds, int loopOffSeconds)
        {
            if (!loopEnabled) return string.Empty;

            if (loopOnSeconds <= 0)
                return "Loop ON seconds must be greater than 0.";

            if (loopOffSeconds <= 0)
                return "Loop OFF seconds must be greater than 0.";

            return string.Empty;
        }

        private async Task<tbl_sensor?> GetActiveSensorWithUtility(Guid sensorId)
        {
            return await db.tbl_sensor
                .Include(x => x.utility)
                .Where(x => x.sensor_id == sensorId && !x.is_deleted)
                .FirstOrDefaultAsync();
        }

        private IQueryable<tbl_hvac_loop_setting> BaseQuery()
        {
            return db.tbl_hvac_loop_setting
                .Include(x => x.sensor)
                    .ThenInclude(x => x.utility)
                .Where(x => !x.is_deleted);
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> AddHvacLoopSetting(AddHvacLoopSettingDTO requestDto)
        {
            try
            {
                if (!IsValidGuid(requestDto.fkSensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid sensor id.",
                        success = false
                    };
                }

                var timingError = ValidateLoopTiming(requestDto.loopEnabled, requestDto.loopOnSeconds, requestDto.loopOffSeconds);
                if (!string.IsNullOrEmpty(timingError))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = timingError,
                        success = false
                    };
                }

                var sensorId = Guid.Parse(requestDto.fkSensor);
                var sensor = await GetActiveSensorWithUtility(sensorId);

                if (sensor == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Sensor not found.",
                        success = false
                    };
                }

                if (!IsHvacSensor(sensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting is only allowed for HVAC sensors.",
                        success = false
                    };
                }

                var existingSetting = await db.tbl_hvac_loop_setting
                    .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingSetting != null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting already exists for this sensor.",
                        success = false
                    };
                }

                var newSetting = mapper.Map<tbl_hvac_loop_setting>(requestDto);
                newSetting.loop_started_at = requestDto.loopEnabled ? DateTime.UtcNow : null;
                newSetting.created_at = DateTime.Now;
                newSetting.updated_at = DateTime.Now;

                await db.tbl_hvac_loop_setting.AddAsync(newSetting);
                await db.SaveChangesAsync();
                await hvacLoopRedisCacheService.SetLoopSettingAsync(newSetting.fk_sensor);


                var saved = await BaseQuery()
                    .Where(x => x.hvac_loop_setting_id == newSetting.hvac_loop_setting_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    data = mapper.Map<HvacLoopSettingResponseDTO>(saved),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> UpdateHvacLoopSetting(UpdateHvacLoopSettingDTO requestDto)
        {
            try
            {
                if (!IsValidGuid(requestDto.hvacLoopSettingId))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid HVAC loop setting id.",
                        success = false
                    };
                }

                var existingSetting = await db.tbl_hvac_loop_setting
                    .Where(x => x.hvac_loop_setting_id == Guid.Parse(requestDto.hvacLoopSettingId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingSetting == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting not found.",
                        success = false
                    };
                }

                var finalSensorId = existingSetting.fk_sensor;

                if (!string.IsNullOrWhiteSpace(requestDto.fkSensor))
                {
                    if (!IsValidGuid(requestDto.fkSensor))
                    {
                        return new ResponseModel<HvacLoopSettingResponseDTO>
                        {
                            remarks = "Invalid sensor id.",
                            success = false
                        };
                    }

                    finalSensorId = Guid.Parse(requestDto.fkSensor);
                }

                var timingError = ValidateLoopTiming(requestDto.loopEnabled, requestDto.loopOnSeconds, requestDto.loopOffSeconds);
                if (!string.IsNullOrEmpty(timingError))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = timingError,
                        success = false
                    };
                }

                var sensor = await GetActiveSensorWithUtility(finalSensorId);

                if (sensor == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Sensor not found.",
                        success = false
                    };
                }

                if (!IsHvacSensor(sensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting is only allowed for HVAC sensors.",
                        success = false
                    };
                }

                var duplicate = await db.tbl_hvac_loop_setting
                    .Where(x => x.fk_sensor == finalSensorId
                             && x.hvac_loop_setting_id != existingSetting.hvac_loop_setting_id
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Another HVAC loop setting already exists for this sensor.",
                        success = false
                    };
                }

                var wasEnabled = existingSetting.loop_enabled;
                mapper.Map(requestDto, existingSetting);

                if (requestDto.loopEnabled && !wasEnabled)
                    existingSetting.loop_started_at = DateTime.UtcNow;

                if (!requestDto.loopEnabled)
                    existingSetting.loop_started_at = null;

                existingSetting.updated_at = DateTime.Now;

                await db.SaveChangesAsync();
                await hvacLoopRedisCacheService.SetLoopSettingAsync(existingSetting.fk_sensor);
                var updated = await BaseQuery()
                    .Where(x => x.hvac_loop_setting_id == existingSetting.hvac_loop_setting_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    data = mapper.Map<HvacLoopSettingResponseDTO>(updated),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<HvacLoopSettingResponseDTO>>> GetAllHvacLoopSettings()
        {
            try
            {
                var settings = await BaseQuery().ToListAsync();

                if (settings.Any())
                {
                    return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                    {
                        data = mapper.Map<List<HvacLoopSettingResponseDTO>>(settings),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                {
                    remarks = "No HVAC loop settings found.",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> GetHvacLoopSettingById(string id)
        {
            try
            {
                if (!IsValidGuid(id))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid id.",
                        success = false
                    };
                }

                var setting = await BaseQuery()
                    .Where(x => x.hvac_loop_setting_id == Guid.Parse(id))
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        data = mapper.Map<HvacLoopSettingResponseDTO>(setting),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "HVAC loop setting not found.",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> GetHvacLoopSettingBySensorId(string sensorId)
        {
            try
            {
                if (!IsValidGuid(sensorId))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid sensor id.",
                        success = false
                    };
                }

                var setting = await BaseQuery()
                    .Where(x => x.fk_sensor == Guid.Parse(sensorId))
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        data = mapper.Map<HvacLoopSettingResponseDTO>(setting),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "No HVAC loop setting found for this sensor.",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<HvacLoopSettingResponseDTO>>> GetHvacLoopSettingsByBusinessId(string businessId)
        {
            try
            {
                if (!IsValidGuid(businessId))
                {
                    return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                    {
                        remarks = "Invalid business id.",
                        success = false
                    };
                }

                var businessGuid = Guid.Parse(businessId);

                var settings = await BaseQuery()
                    .Include(x => x.sensor.device)
                    .Where(x => x.sensor.device.fk_business == businessGuid)
                    .ToListAsync();

                if (settings.Any())
                {
                    return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                    {
                        data = mapper.Map<List<HvacLoopSettingResponseDTO>>(settings),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                {
                    remarks = "No HVAC loop settings found.",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<HvacLoopSettingResponseDTO>>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> EnableLoop(HvacLoopSensorDTO requestDto)
        {
            try
            {
                if (!IsValidGuid(requestDto.fkSensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid sensor id.",
                        success = false
                    };
                }

                var sensorId = Guid.Parse(requestDto.fkSensor);
                var sensor = await GetActiveSensorWithUtility(sensorId);

                if (sensor == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Sensor not found.",
                        success = false
                    };
                }

                if (!IsHvacSensor(sensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting is only allowed for HVAC sensors.",
                        success = false
                    };
                }

                var setting = await db.tbl_hvac_loop_setting
                    .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting not found. Please create loop setting first.",
                        success = false
                    };
                }

                var timingError = ValidateLoopTiming(true, setting.loop_on_seconds, setting.loop_off_seconds);
                if (!string.IsNullOrEmpty(timingError))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = timingError,
                        success = false
                    };
                }

                setting.loop_enabled = true;
                setting.loop_started_at = DateTime.UtcNow;
                setting.updated_at = DateTime.Now;

                await db.SaveChangesAsync();
                await hvacLoopRedisCacheService.SetLoopSettingAsync(setting.fk_sensor);
                var updated = await BaseQuery()
                    .Where(x => x.hvac_loop_setting_id == setting.hvac_loop_setting_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    data = mapper.Map<HvacLoopSettingResponseDTO>(updated),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<HvacLoopSettingResponseDTO>> DisableLoop(HvacLoopSensorDTO requestDto)
        {
            try
            {
                if (!IsValidGuid(requestDto.fkSensor))
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "Invalid sensor id.",
                        success = false
                    };
                }

                var setting = await db.tbl_hvac_loop_setting
                    .Where(x => x.fk_sensor == Guid.Parse(requestDto.fkSensor) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return new ResponseModel<HvacLoopSettingResponseDTO>
                    {
                        remarks = "HVAC loop setting not found.",
                        success = false
                    };
                }

                setting.loop_enabled = false;
                setting.loop_started_at = null;
                setting.updated_at = DateTime.Now;

                await db.SaveChangesAsync();
                await hvacLoopRedisCacheService.SetLoopSettingAsync(setting.fk_sensor);
                var updated = await BaseQuery()
                    .Where(x => x.hvac_loop_setting_id == setting.hvac_loop_setting_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    data = mapper.Map<HvacLoopSettingResponseDTO>(updated),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<HvacLoopSettingResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteHvacLoopSettingById(string id)
        {
            try
            {
                if (!IsValidGuid(id))
                {
                    return new ResponseModel
                    {
                        remarks = "Invalid id.",
                        success = false
                    };
                }

                var setting = await db.tbl_hvac_loop_setting
                    .Where(x => x.hvac_loop_setting_id == Guid.Parse(id) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    setting.loop_enabled = false;
                    setting.loop_started_at = null;
                    setting.is_deleted = true;
                    setting.updated_at = DateTime.Now;

                    await db.SaveChangesAsync();
                    await hvacLoopRedisCacheService.DeleteLoopSettingAsync(setting.fk_sensor);
                    return new ResponseModel
                    {
                        remarks = "HVAC loop setting deleted successfully.",
                        success = true
                    };
                }

                return new ResponseModel
                {
                    remarks = "HVAC loop setting not found.",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }
    }
}
