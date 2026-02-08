//using EMO.Models.DBModels.DBTables;
//using EMO.Models.DTOs.DeviceTypeDTOs;
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using EMO.Models.DBModels;
//using EMO.Models.DTOs.ResponseDTO;

//namespace EMO.Repositories.DeviceTypeServicesRepo
//{
//    public class DeviceTypeServices : IDeviceTypeServices
//    {
//        private readonly DBUserManagementContext db;
//        private readonly IMapper mapper;

//        public DeviceTypeServices(DBUserManagementContext db, IMapper mapper)
//        {
//            this.db = db;
//            this.mapper = mapper;
//        }

//        public async Task<ResponseModel<DeviceTypeResponseDTO>> AddDeviceType(AddDeviceTypeDTO requestDto)
//        {
//            try
//            {
//                var existingDeviceType = await db.tbl_device_type
//                    .Where(x => x.device_type_name.ToLower() == requestDto.deviceTypeName.ToLower())
//                    .FirstOrDefaultAsync();

//                if (existingDeviceType == null)
//                {
//                    var newDeviceType = mapper.Map<tbl_device_type>(requestDto);
//                    await db.tbl_device_type.AddAsync(newDeviceType);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        data = mapper.Map<DeviceTypeResponseDTO>(newDeviceType),
//                        remarks = "Success",
//                        success = true
//                    };
//                }
//                else
//                {
//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        remarks = "Device Type Already Exists",
//                        success = false
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<DeviceTypeResponseDTO>> UpdateDeviceType(UpdateDeviceTypeDTO requestDto)
//        {
//            try
//            {
//                var existingDeviceType = await db.tbl_device_type
//                    .Where(x => x.device_type_id == Guid.Parse(requestDto.deviceTypeId))
//                    .FirstOrDefaultAsync();

//                if (existingDeviceType != null)
//                {
//                    mapper.Map(requestDto, existingDeviceType);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        data = mapper.Map<DeviceTypeResponseDTO>(existingDeviceType),
//                        remarks = "Success",
//                        success = true
//                    };
//                }
//                else
//                {
//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        remarks = "No Record Found",
//                        success = false
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<DeviceTypeResponseDTO>> GetDeviceTypeById(string deviceTypeId)
//        {
//            try
//            {
//                var deviceType = await db.tbl_device_type
//                    .Where(x => x.device_type_id == Guid.Parse(deviceTypeId))
//                    .FirstOrDefaultAsync();

//                if (deviceType != null)
//                {
//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        data = mapper.Map<DeviceTypeResponseDTO>(deviceType),
//                        remarks = "Success",
//                        success = true
//                    };
//                }
//                else
//                {
//                    return new ResponseModel<DeviceTypeResponseDTO>()
//                    {
//                        remarks = "Device Type not found",
//                        success = false
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel<List<DeviceTypeResponseDTO>>> GetAllDeviceTypes()
//        {
//            try
//            {
//                var deviceTypes = await db.tbl_device_type.ToListAsync();

//                if (deviceTypes.Any())
//                {
//                    return new ResponseModel<List<DeviceTypeResponseDTO>>()
//                    {
//                        data = mapper.Map<List<DeviceTypeResponseDTO>>(deviceTypes),
//                        remarks = "Success",
//                        success = true
//                    };
//                }
//                else
//                {
//                    return new ResponseModel<List<DeviceTypeResponseDTO>>()
//                    {
//                        remarks = "No Device Type found",
//                        success = false
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new ResponseModel<List<DeviceTypeResponseDTO>>()
//                {
//                    remarks = $"There was a fatal error: {ex}",
//                    success = false
//                };
//            }
//        }

//        public async Task<ResponseModel> DeleteDeviceTypeById(string deviceTypeId)
//        {
//            try
//            {
//                var deviceType = await db.tbl_device_type.FindAsync(Guid.Parse(deviceTypeId));

//                if (deviceType != null)
//                {
//                    db.tbl_device_type.Remove(deviceType);
//                    await db.SaveChangesAsync();

//                    return new ResponseModel()
//                    {
//                        remarks = "Device Type deleted successfully",
//                        success = true
//                    };
//                }
//                else
//                {
//                    return new ResponseModel()
//                    {
//                        remarks = "Device Type not found",
//                        success = false
//                    };
//                }
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
