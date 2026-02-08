//// ========================= Repository =========================
//using EMO.Models.DBModels.DBTables;
//using EMO.Models.DTOs.SensorTypeDTOs;
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using EMO.Models.DBModels;
//using EMO.Models.DTOs.ResponseDTO;

//namespace EMO.Repositories.SensorTypeServicesRepo
//{
//    public class SensorTypeServices : ISensortypeServices
//    {
//        private readonly DBUserManagementContext db;
//        private readonly IMapper mapper;

//        public SensorTypeServices(DBUserManagementContext db, IMapper mapper)
//        {
//            this.db = db;
//            this.mapper = mapper;
//        }

//        public async Task<ResponseModel<SensorTypeResponseDTO>> AddSensorType(AddSensorTypeDTO requestDto)
//        {
//            try
//            {
//                var existing = await db.tbl_sensor_type
//                    .Where(x => x.sensor_type_name.ToLower() == requestDto.sensorTypeName.ToLower())
//                    .FirstOrDefaultAsync();

//                if (existing == null)
//                {
//                    var newItem = mapper.Map<tbl_sensor_type>(requestDto);
//                    await db.tbl_sensor_type.AddAsync(newItem);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel<SensorTypeResponseDTO>()
//                    {
//                        data = mapper.Map<SensorTypeResponseDTO>(newItem),
//                        remarks = "Success",
//                        success = true
//                    };
//                }

//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = "Sensor Type Already Exists",
//                    success = false
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<SensorTypeResponseDTO>> UpdateSensorType(UpdateSensorTypeDTO requestDto)
//        {
//            try
//            {
//                var existing = await db.tbl_sensor_type
//                    .Where(x => x.sensor_type_id == Guid.Parse(requestDto.sensorTypeId))
//                    .FirstOrDefaultAsync();

//                if (existing != null)
//                {
//                    mapper.Map(requestDto, existing);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel<SensorTypeResponseDTO>()
//                    {
//                        data = mapper.Map<SensorTypeResponseDTO>(existing),
//                        remarks = "Success",
//                        success = true
//                    };
//                }

//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = "No Record Found",
//                    success = false
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<SensorTypeResponseDTO>> GetSensorTypeById(string sensorTypeId)
//        {
//            try
//            {
//                var item = await db.tbl_sensor_type
//                    .Where(x => x.sensor_type_id == Guid.Parse(sensorTypeId))
//                    .FirstOrDefaultAsync();

//                if (item != null)
//                {
//                    return new ResponseModel<SensorTypeResponseDTO>()
//                    {
//                        data = mapper.Map<SensorTypeResponseDTO>(item),
//                        remarks = "Success",
//                        success = true
//                    };
//                }

//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = "Sensor Type not found",
//                    success = false
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<SensorTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<List<SensorTypeResponseDTO>>> GetAllSensorTypes()
//        {
//            try
//            {
//                var items = await db.tbl_sensor_type.ToListAsync();

//                if (items.Any())
//                {
//                    return new ResponseModel<List<SensorTypeResponseDTO>>()
//                    {
//                        data = mapper.Map<List<SensorTypeResponseDTO>>(items),
//                        remarks = "Success",
//                        success = true
//                    };
//                }

//                return new ResponseModel<List<SensorTypeResponseDTO>>()
//                {
//                    remarks = "No Sensor Type found",
//                    success = false
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<List<SensorTypeResponseDTO>>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel> DeleteSensorTypeById(string sensorTypeId)
//        {
//            try
//            {
//                var item = await db.tbl_sensor_type.FindAsync(Guid.Parse(sensorTypeId));

//                if (item != null)
//                {
//                    db.tbl_sensor_type.Remove(item);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel()
//                    {
//                        remarks = "Sensor Type deleted successfully",
//                        success = true
//                    };
//                }

//                return new ResponseModel()
//                {
//                    remarks = "Sensor Type not found",
//                    success = false
//                };
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }
//    }
//}
