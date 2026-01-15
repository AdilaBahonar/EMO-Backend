using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.DeviceDTOs;
using EMO.Models.DTOs.UserTypeDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class DeviceMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public DeviceMapper()
        {
            CreateMap<AddDeviceDTO, tbl_device>()
             .ForMember(d => d.device_name, opt => opt.MapFrom(src => src.deviceName))
             .ForMember(d => d.is_active, opt => opt.MapFrom(src => (src.isActive)));
            CreateMap<UpdateDeviceDTO, tbl_device>()
              .ForMember(d => d.device_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.deviceName) ? src.deviceName : dest.device_name))
              .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => (src.isActive) ? (src.isActive) : dest.is_active));
            CreateMap<tbl_device, DeviceResponseDTO>()
              .ForMember(d => d.deviceId, opt => opt.MapFrom(src => src.device_id.ToString()))
              .ForMember(d => d.deviceName, opt => opt.MapFrom(src => src.device_name))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
        }

    }
}
