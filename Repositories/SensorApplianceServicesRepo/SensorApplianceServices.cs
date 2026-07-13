using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ApplianceDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorApplianceDTOs;
using EMO.Repositories.SensorsChainRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.SensorApplianceServicesRepo
{
    public class SensorApplianceServices : ISensorApplianceServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly ISensorRedisCacheService sensorRedisCacheService;

        public SensorApplianceServices(DBUserManagementContext db, IMapper mapper, ISensorRedisCacheService sensorRedisCacheService)
        {
            this.db = db;
            this.mapper = mapper;
            this.sensorRedisCacheService = sensorRedisCacheService;
        }

        public async Task<ResponseModel<SensorApplianceResponseDTO>> AssignApplianceToSensor(AssignSensorApplianceDTO requestDto)
        {
            try
            {
                var sensorId = Guid.Parse(requestDto.fkSensor);
                var applianceId = Guid.Parse(requestDto.fkAppliance);

                var sensor = await db.tbl_sensor
                    .Include(x => x.utility)
                    .Include(x => x.device)
                    .Where(x => x.sensor_id == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (sensor == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Sensor not found",
                        success = false
                    };
                }

                var appliance = await db.tbl_business_appliance
                    .Where(x => x.business_appliance_id == applianceId
                             && x.fk_business == sensor.device.fk_business
                             && x.is_active
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Business appliance not found for this sensor business",
                        success = false
                    };
                }

                if (sensor.fk_utility != appliance.fk_utility)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Selected appliance utility does not match sensor utility",
                        success = false
                    };
                }

                // One active appliance assignment per sensor.
                // If already assigned, update the existing assignment instead of creating duplicates.
                var existingAssignment = await db.tbl_sensor_appliance
                    .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null)
                {
                    existingAssignment.fk_appliance = applianceId;
                    existingAssignment.remarks = requestDto.remarks;
                    existingAssignment.is_active = requestDto.isActive;
                    existingAssignment.updated_at = DateTime.Now;
                    existingAssignment.assigned_at = DateTime.Now;
                }
                else
                {
                    var newAssignment = mapper.Map<tbl_sensor_appliance>(requestDto);
                    await db.tbl_sensor_appliance.AddAsync(newAssignment);
                }

                await db.SaveChangesAsync();
                await sensorRedisCacheService.SetSensorChainAsync(sensorId);

                var saved = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.fk_sensor == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    data = mapper.Map<SensorApplianceResponseDTO>(saved),
                    remarks = existingAssignment != null ? "Sensor appliance assignment updated successfully" : "Appliance assigned to sensor successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorApplianceResponseDTO>> UpdateSensorAppliance(UpdateSensorApplianceDTO requestDto)
        {
            try
            {
                var existingAssignment = await db.tbl_sensor_appliance
                    .Where(x => x.sensor_appliance_id == Guid.Parse(requestDto.sensorApplianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAssignment == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Sensor appliance assignment not found",
                        success = false
                    };
                }

                Guid finalSensorId = string.IsNullOrWhiteSpace(requestDto.fkSensor)
                    ? existingAssignment.fk_sensor
                    : Guid.Parse(requestDto.fkSensor);

                Guid finalApplianceId = string.IsNullOrWhiteSpace(requestDto.fkAppliance)
                    ? existingAssignment.fk_appliance
                    : Guid.Parse(requestDto.fkAppliance);

                var sensor = await db.tbl_sensor
                    .Include(x => x.device)
                    .Where(x => x.sensor_id == finalSensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (sensor == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Sensor not found",
                        success = false
                    };
                }

                var appliance = await db.tbl_business_appliance
                    .Where(x => x.business_appliance_id == finalApplianceId
                             && x.fk_business == sensor.device.fk_business
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Business appliance not found for this sensor business",
                        success = false
                    };
                }

                if (sensor.fk_utility != appliance.fk_utility)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Selected appliance utility does not match sensor utility",
                        success = false
                    };
                }

                mapper.Map(requestDto, existingAssignment);
                existingAssignment.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                await sensorRedisCacheService.SetSensorChainAsync(existingAssignment.fk_sensor);
                var saved = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.sensor_appliance_id == existingAssignment.sensor_appliance_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    data = mapper.Map<SensorApplianceResponseDTO>(saved),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorApplianceResponseDTO>> GetSensorApplianceById(string sensorApplianceId)
        {
            try
            {
                var assignment = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.sensor_appliance_id == Guid.Parse(sensorApplianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "Sensor appliance assignment not found",
                        success = false
                    };
                }

                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    data = mapper.Map<SensorApplianceResponseDTO>(assignment),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorApplianceResponseDTO>> GetActiveApplianceBySensorId(string sensorId)
        {
            try
            {
                var assignment = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.fk_sensor == Guid.Parse(sensorId) && x.is_active && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return new ResponseModel<SensorApplianceResponseDTO>()
                    {
                        remarks = "No appliance assigned to this sensor",
                        success = false
                    };
                }

                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    data = mapper.Map<SensorApplianceResponseDTO>(assignment),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }


        public async Task<ResponseModel<SensorAssignableAppliancesDTO>> GetAssignableBusinessAppliancesBySensorId(string sensorId)
        {
            try
            {
                var sensorGuid = Guid.Parse(sensorId);

                var sensor = await db.tbl_sensor
                    .Include(x => x.utility)
                    .Include(x => x.device)
                    .Where(x => x.sensor_id == sensorGuid && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (sensor == null)
                {
                    return new ResponseModel<SensorAssignableAppliancesDTO>()
                    {
                        remarks = "Sensor not found",
                        success = false
                    };
                }

                if (sensor.device == null || sensor.device.fk_business == Guid.Empty)
                {
                    return new ResponseModel<SensorAssignableAppliancesDTO>()
                    {
                        remarks = "Sensor business could not be resolved from its device",
                        success = false
                    };
                }

                var businessId = sensor.device.fk_business;
                var utilityId = sensor.fk_utility;

                var appliances = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.fk_business == businessId
                             && x.fk_utility == utilityId
                             && x.is_active
                             && !x.is_deleted)
                    .OrderBy(x => x.appliance_name)
                    .ToListAsync();

                if (!appliances.Any())
                {
                    await AddMissingDefaultAppliancesToBusinessAsync(businessId);
                    await db.SaveChangesAsync();

                    appliances = await db.tbl_business_appliance
                        .Include(x => x.business)
                        .Include(x => x.utility)
                        .Where(x => x.fk_business == businessId
                                 && x.fk_utility == utilityId
                                 && x.is_active
                                 && !x.is_deleted)
                        .OrderBy(x => x.appliance_name)
                        .ToListAsync();

                }

                var response = new SensorAssignableAppliancesDTO
                {
                    fkSensor = sensor.sensor_id.ToString(),
                    sensorName = sensor.sensor_name,
                    fkBusiness = businessId.ToString(),
                    businessName = appliances.FirstOrDefault()?.business?.business_name ?? string.Empty,
                    fkUtility = utilityId.ToString(),
                    utilityName = sensor.utility?.utility_name ?? appliances.FirstOrDefault()?.utility?.utility_name ?? string.Empty,
                    appliances = mapper.Map<List<ApplianceResponseDTO>>(appliances)
                };

                return new ResponseModel<SensorAssignableAppliancesDTO>()
                {
                    data = response,
                    remarks = appliances.Any()
                        ? "Success"
                        : "No business appliance found for this sensor utility",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<SensorAssignableAppliancesDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SensorApplianceResponseDTO>>> GetAllSensorAppliances()
        {
            try
            {
                var assignments = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => !x.is_deleted)
                    .OrderBy(x => x.appliance.appliance_name)
                    .ToListAsync();

                if (assignments.Any())
                {
                    return new ResponseModel<List<SensorApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<SensorApplianceResponseDTO>>(assignments),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<SensorApplianceResponseDTO>>()
                {
                    remarks = "No sensor appliance assignment found",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<SensorApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SensorApplianceResponseDTO>>> GetSensorAppliancesByApplianceId(string applianceId)
        {
            try
            {
                var assignments = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.fk_appliance == Guid.Parse(applianceId) && !x.is_deleted)
                    .ToListAsync();

                if (assignments.Any())
                {
                    return new ResponseModel<List<SensorApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<SensorApplianceResponseDTO>>(assignments),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<SensorApplianceResponseDTO>>()
                {
                    remarks = "No sensor assignment found for this appliance",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<SensorApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceStatusDTO>> GetSensorApplianceStatus(string sensorId)
        {
            try
            {
                var assignment = await db.tbl_sensor_appliance
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.device)
                    .Include(x => x.appliance)
                    .ThenInclude(x => x.utility)
                    .Where(x => x.fk_sensor == Guid.Parse(sensorId) && x.is_active && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return new ResponseModel<ApplianceStatusDTO>()
                    {
                        remarks = "No appliance assigned to this sensor",
                        success = false
                    };
                }

                var latestReading = await db.tbl_singal_phase_data
                    .Where(x => x.fk_sensor == assignment.fk_sensor)
                    .OrderByDescending(x => x.created_at)
                    .FirstOrDefaultAsync();

                if (latestReading == null)
                {
                    return new ResponseModel<ApplianceStatusDTO>()
                    {
                        data = new ApplianceStatusDTO
                        {
                            sensorId = assignment.fk_sensor.ToString(),
                            sensorName = assignment.sensor.sensor_name,
                            applianceId = assignment.fk_appliance.ToString(),
                            applianceName = assignment.appliance.appliance_name,
                            companyName = assignment.appliance.company_name,
                            modelNumber = assignment.appliance.model_number,
                            utilityName = assignment.appliance.utility.utility_name,
                            minCurrent = assignment.appliance.min_current,
                            maxCurrent = assignment.appliance.max_current,
                            minPower = assignment.appliance.min_power,
                            maxPower = assignment.appliance.max_power,
                            standbyPower = assignment.appliance.standby_power,
                            normalPowerFactor = assignment.appliance.normal_power_factor,
                            status = "No Data",
                            alertMessage = "No reading found for this sensor"
                        },
                        remarks = "No reading found",
                        success = true
                    };
                }

                float actualCurrent = latestReading.current;
                float actualPower = latestReading.active_power;
                float actualPowerFactor = latestReading.power_factor;

                string status = "Normal";
                string alertMessage = "Appliance is working within expected range";

                if (assignment.appliance.max_current > 0 && actualCurrent > assignment.appliance.max_current)
                {
                    status = "Overload";
                    alertMessage = "Current is higher than the expected appliance range";
                }
                else if (assignment.appliance.max_power > 0 && actualPower > assignment.appliance.max_power)
                {
                    status = "High Consumption";
                    alertMessage = "Active power is higher than the expected appliance range";
                }
                else if (assignment.appliance.standby_power > 0 && actualPower > 0 && actualPower <= assignment.appliance.standby_power)
                {
                    status = "Idle";
                    alertMessage = "Appliance is consuming standby power and may be idle";
                }
                else if (assignment.appliance.normal_power_factor > 0 && actualPowerFactor > 0 && actualPowerFactor < assignment.appliance.normal_power_factor)
                {
                    status = "Low Power Factor";
                    alertMessage = "Power factor is lower than expected for this appliance";
                }

                return new ResponseModel<ApplianceStatusDTO>()
                {
                    data = new ApplianceStatusDTO
                    {
                        sensorId = assignment.fk_sensor.ToString(),
                        sensorName = assignment.sensor.sensor_name,
                        applianceId = assignment.fk_appliance.ToString(),
                        applianceName = assignment.appliance.appliance_name,
                        companyName = assignment.appliance.company_name,
                        modelNumber = assignment.appliance.model_number,
                        utilityName = assignment.appliance.utility.utility_name,
                        actualCurrent = actualCurrent,
                        actualPower = actualPower,
                        actualPowerFactor = actualPowerFactor,
                        minCurrent = assignment.appliance.min_current,
                        maxCurrent = assignment.appliance.max_current,
                        minPower = assignment.appliance.min_power,
                        maxPower = assignment.appliance.max_power,
                        standbyPower = assignment.appliance.standby_power,
                        normalPowerFactor = assignment.appliance.normal_power_factor,
                        status = status,
                        alertMessage = alertMessage,
                        lastReadingAt = latestReading.created_at.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceStatusDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteSensorApplianceById(string sensorApplianceId)
        {
            try
            {
                var assignment = await db.tbl_sensor_appliance
                    .Where(x => x.sensor_appliance_id == Guid.Parse(sensorApplianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return new ResponseModel()
                    {
                        remarks = "Sensor appliance assignment not found",
                        success = false
                    };
                }

                assignment.is_deleted = true;
                assignment.is_active = false;
                assignment.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                await sensorRedisCacheService.SetSensorChainAsync(assignment.fk_sensor);

                return new ResponseModel()
                {
                    remarks = "Sensor appliance assignment deleted successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        private async Task AddMissingDefaultAppliancesToBusinessAsync(Guid businessId)
        {
            var defaultAppliances = await db.tbl_appliance
                .Where(x => x.is_default && x.is_active && !x.is_deleted)
                .ToListAsync();

            var copiedDefaultIds = await db.tbl_business_appliance
                .Where(x => x.fk_business == businessId && x.fk_appliance.HasValue && !x.is_deleted)
                .Select(x => x.fk_appliance!.Value)
                .ToListAsync();

            foreach (var defaultAppliance in defaultAppliances)
            {
                if (copiedDefaultIds.Contains(defaultAppliance.appliance_id))
                    continue;

                await db.tbl_business_appliance.AddAsync(CreateBusinessApplianceFromDefault(businessId, defaultAppliance));
            }
        }

        private static tbl_business_appliance CreateBusinessApplianceFromDefault(Guid businessId, tbl_appliance defaultAppliance)
        {
            return new tbl_business_appliance
            {
                fk_business = businessId,
                fk_appliance = defaultAppliance.appliance_id,
                appliance_name = defaultAppliance.appliance_name,
                company_name = defaultAppliance.company_name,
                model_number = defaultAppliance.model_number,
                rated_voltage = defaultAppliance.rated_voltage,
                min_current = defaultAppliance.min_current,
                max_current = defaultAppliance.max_current,
                min_power = defaultAppliance.min_power,
                max_power = defaultAppliance.max_power,
                standby_power = defaultAppliance.standby_power,
                normal_power_factor = defaultAppliance.normal_power_factor,
                description = defaultAppliance.description,
                priority_level = defaultAppliance.priority_level,
                is_critical = defaultAppliance.is_critical,
                is_default = true,
                is_custom = false,
                is_active = defaultAppliance.is_active,
                fk_utility = defaultAppliance.fk_utility
            };
        }
    }
}
