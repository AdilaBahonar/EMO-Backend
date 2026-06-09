using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorDTOs;
using EMO.Repositories.SensorsChainRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.SensorServicesRepo
{
    public class SensorServices : ISensorServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly ISensorRedisCacheService sensorRedisCacheService;

        public SensorServices(DBUserManagementContext db, IMapper mapper, ISensorRedisCacheService sensorRedisCacheService)
        {
            this.db = db;
            this.mapper = mapper;
            this.sensorRedisCacheService = sensorRedisCacheService;
        }

        public async Task<ResponseModel<SensorResponseDTO>> AddSensor(AddSensorDTO requestDto)
        {
            try
            {
                var existingSensor = await db.tbl_sensor
                    .Where(x => x.sensor_name.ToLower() == requestDto.sensorName.ToLower() && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingSensor == null)
                {
                    var newSensor = mapper.Map<tbl_sensor>(requestDto);
                    await db.tbl_sensor.AddAsync(newSensor);
                    await db.SaveChangesAsync();

                    await sensorRedisCacheService.SetSensorChainAsync(newSensor.sensor_id);

                    return new ResponseModel<SensorResponseDTO>()
                    {
                        data = mapper.Map<SensorResponseDTO>(newSensor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SensorResponseDTO>()
                    {
                        remarks = "Sensor Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SensorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorResponseDTO>> UpdateSensor(UpdateSensorDTO requestDto)
        {
            try
            {
                var sensorId = Guid.Parse(requestDto.sensorId);
                var existingSensor = await db.tbl_sensor
                    .Where(x => x.sensor_id == sensorId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingSensor != null)
                {
                    mapper.Map(requestDto, existingSensor);
                    await db.SaveChangesAsync();
                    await sensorRedisCacheService.SetSensorChainAsync(sensorId);


                    return new ResponseModel<SensorResponseDTO>()
                    {
                        data = mapper.Map<SensorResponseDTO>(existingSensor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SensorResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SensorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorResponseDTO>> GetSensorById(string sensorId)
        {
            try
            {
                var sensor = await db.tbl_sensor.Include(x => x.device).Include(x => x.utility)
                    .Where(x => x.sensor_id == Guid.Parse(sensorId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (sensor != null)
                {
                    return new ResponseModel<SensorResponseDTO>()
                    {
                        data = mapper.Map<SensorResponseDTO>(sensor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SensorResponseDTO>()
                    {
                        remarks = "Sensor not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SensorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SensorResponseDTO>>> GetAllSensors()
        {
            try
            {
                var sensors = await db.tbl_sensor.Where(x=>!x.is_deleted).Include(x => x.device).Include(x => x.utility).ToListAsync();

                if (sensors.Any())
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        data = mapper.Map<List<SensorResponseDTO>>(sensors),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        remarks = "No Sensor found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SensorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SensorResponseDTO>>> GetSensorsByBusinessId(string businessId)
        {
            try
            {
                var sensors = await db.tbl_sensor.Include(x => x.device).Include(x => x.utility).Where(x=>x.device.fk_business == Guid.Parse(businessId) && !x.is_deleted).ToListAsync();

                if (sensors.Any())
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        data = mapper.Map<List<SensorResponseDTO>>(sensors),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        remarks = "No record found.",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SensorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }


        public async Task<ResponseModel<List<SensorResponseDTO>>> GetSensorsByDeviceId(string deviceId)
        {
            try
            {
                var sensors = await db.tbl_sensor.Include(x => x.device).Include(x => x.utility).Where(x => x.fk_device == Guid.Parse(deviceId) && !x.is_deleted).ToListAsync();

                if (sensors.Any())
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        data = mapper.Map<List<SensorResponseDTO>>(sensors),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<SensorResponseDTO>>()
                    {
                        remarks = "No record found.",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SensorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> DeleteSensorById(string sensorId)
        {
            try
            {
                var parsedSensorId = Guid.Parse(sensorId);

                var sensor = await db.tbl_sensor.Where(x=>x.sensor_id == parsedSensorId && !x.is_deleted).FirstOrDefaultAsync();

                if (sensor != null)
                {
                    sensor.is_deleted = true;
                    await db.SaveChangesAsync();

                    await sensorRedisCacheService.DeleteSensorChainAsync(parsedSensorId);

                    return new ResponseModel()
                    {
                        remarks = "Sensor deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Sensor not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
    }
}
