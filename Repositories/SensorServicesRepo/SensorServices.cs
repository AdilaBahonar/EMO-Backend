using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SensorDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SensorServicesRepo
{
    public class SensorServices : ISensorServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public SensorServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<SensorResponseDTO>> AddSensor(AddSensorDTO requestDto)
        {
            try
            {
                var existingSensor = await db.tbl_sensor
                    .Where(x => x.sensor_name.ToLower() == requestDto.sensorName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingSensor == null)
                {
                    var newSensor = mapper.Map<tbl_sensor>(requestDto);
                    await db.tbl_sensor.AddAsync(newSensor);
                    await db.SaveChangesAsync();

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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorResponseDTO>> UpdateSensor(UpdateSensorDTO requestDto)
        {
            try
            {
                var existingSensor = await db.tbl_sensor
                    .Where(x => x.sensor_id == Guid.Parse(requestDto.sensorId))
                    .FirstOrDefaultAsync();

                if (existingSensor != null)
                {
                    mapper.Map(requestDto, existingSensor);
                    await db.SaveChangesAsync();

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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SensorResponseDTO>> GetSensorById(string sensorId)
        {
            try
            {
                var sensor = await db.tbl_sensor
                    .Where(x => x.sensor_id == Guid.Parse(sensorId))
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SensorResponseDTO>>> GetAllSensors()
        {
            try
            {
                var sensors = await db.tbl_sensor.ToListAsync();

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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteSensorById(string sensorId)
        {
            try
            {
                var sensor = await db.tbl_sensor.FindAsync(Guid.Parse(sensorId));

                if (sensor != null)
                {
                    db.tbl_sensor.Remove(sensor);
                    await db.SaveChangesAsync();

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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
    }
}
