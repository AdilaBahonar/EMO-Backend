using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.DeviceDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using APIProduct.Repositories.DeviceServicesRepo;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.DeviceServicesRepo
{
    public class DeviceServices : IDeviceServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public DeviceServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<DeviceResponseDTO>> AddDevice(AddDeviceDTO requestDto)
        {
            try
            {
                var existingDevice = await db.tbl_device
                    .Where(x => x.device_name.ToLower() == requestDto.deviceName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingDevice == null)
                {
                    var newDevice = mapper.Map<tbl_device>(requestDto);
                    await db.tbl_device.AddAsync(newDevice);
                    await db.SaveChangesAsync();

                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        data = mapper.Map<DeviceResponseDTO>(newDevice),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        remarks = "Device Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<DeviceResponseDTO>> UpdateDevice(UpdateDeviceDTO requestDto)
        {
            try
            {
                var existingDevice = await db.tbl_device
                    .Where(x => x.device_id == Guid.Parse(requestDto.deviceId))
                    .FirstOrDefaultAsync();

                if (existingDevice != null)
                {
                    mapper.Map(requestDto, existingDevice);
                    await db.SaveChangesAsync();

                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        data = mapper.Map<DeviceResponseDTO>(existingDevice),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<DeviceResponseDTO>> GetDeviceById(string deviceId)
        {
            try
            {
                var device = await db.tbl_device
                    .Where(x => x.device_id == Guid.Parse(deviceId))
                    .FirstOrDefaultAsync();

                if (device != null)
                {
                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        data = mapper.Map<DeviceResponseDTO>(device),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<DeviceResponseDTO>()
                    {
                        remarks = "Device not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<DeviceResponseDTO>>> GetAllDevices()
        {
            try
            {
                var devices = await db.tbl_device.ToListAsync();

                if (devices.Any())
                {
                    return new ResponseModel<List<DeviceResponseDTO>>()
                    {
                        data = mapper.Map<List<DeviceResponseDTO>>(devices),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<DeviceResponseDTO>>()
                    {
                        remarks = "No Device found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<DeviceResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteDeviceById(string deviceId)
        {
            try
            {
                var device = await db.tbl_device.FindAsync(Guid.Parse(deviceId));

                if (device != null)
                {
                    db.tbl_device.Remove(device);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Device deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Device not found",
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
