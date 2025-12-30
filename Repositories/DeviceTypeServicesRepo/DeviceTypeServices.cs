using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BuildingDTOs;
using EMO.Models.DTOs.DeviceDTOs;
using EMO.Models.DTOs.DeviceTypeDTOs;
using EMO.Repositories.DeviceServicesRepo;
using EMO.Repositories.DeviceTypeServicesRepo;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BuildingServicesRepo
{
    public class DeviceTypeServices : IDeviceTypeServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public DeviceTypeServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<DeviceTypeResponseDTO>> AddDeviceType(AddDeviceTypeDTO requestDto)
        {
            try
            {
                var existingDevicetype = await db.tbl_device_type
                    .Where(b => b.device_type_name.ToLower() == requestDto.deviceTypeName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingDevicetype == null)
                {
                    var newDevicetype = mapper.Map<tbl_device_type>(requestDto);
                    await db.tbl_device_type.AddAsync(newDevicetype);
                    await db.SaveChangesAsync();

                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        data = mapper.Map<DeviceTypeResponseDTO>(newDevicetype),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        remarks = "Device Type Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<DeviceTypeResponseDTO>> UpdateDeviceType(UpdateDeviceTypeDTO requestDto)
        {
            try
            {
                var existingDevicetype = await db.tbl_device_type
                    .Where(b => b.device_type_id == Guid.Parse(requestDto.deviceTypeId))
                    .FirstOrDefaultAsync();

                if (existingDevicetype != null)
                {
                    mapper.Map(requestDto, existingDevicetype);
                    await db.SaveChangesAsync();

                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        data = mapper.Map<DeviceTypeResponseDTO>(existingDevicetype),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<DeviceTypeResponseDTO>> GetDeviceTypeById(string DeviceTypeId)
        {
            try
            {
                var devicetype = await db.tbl_device_type
                    .Where(b => b.device_type_id == Guid.Parse(DeviceTypeId))
                    .FirstOrDefaultAsync();

                if (devicetype != null)
                {
                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        data = mapper.Map<DeviceTypeResponseDTO>(devicetype),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceTypeResponseDTO>()
                    {
                        remarks = "Device Type not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<DeviceTypeResponseDTO>>> GetAllDeviceTypes()
        {
            try
            {
                var devicetypes = await db.tbl_device_type.ToListAsync();

                if (devicetypes.Any())
                {
                    return new ResponseModel<List<DeviceTypeResponseDTO>>()
                    {
                        data = mapper.Map<List<DeviceTypeResponseDTO>>(devicetypes),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<DeviceTypeResponseDTO>>()
                    {
                        remarks = "No Device Type found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<DeviceTypeResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteDeviceTypeById(string DeviceTypeId)
        {
            try
            {
                var devicetype = await db.tbl_device.FindAsync(Guid.Parse(DeviceTypeId));

                if (devicetype != null)
                {
                    db.tbl_device.Remove(devicetype);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Device Type deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Device Type not found",
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
