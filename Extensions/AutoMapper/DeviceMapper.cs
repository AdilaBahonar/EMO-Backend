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
             .ForMember(d => d.fk_device_type, opt => opt.MapFrom(src => Guid.Parse(src.fkDeviceType)))
             .ForMember(d => d.fk_control_type, opt => opt.MapFrom(src => Guid.Parse(src.fkControlType)))
             .ForMember(d => d.fk_office, opt => opt.MapFrom(src => Guid.Parse(src.fkOffice)));
            CreateMap<UpdateDeviceDTO, tbl_device>()
              .ForMember(d => d.device_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.deviceName) ? src.deviceName : dest.device_name))
              .ForMember(d => d.fk_device_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkDeviceType) ? Guid.Parse(src.fkDeviceType) : dest.fk_device_type))
              .ForMember(d => d.fk_office, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkOffice) ? Guid.Parse(src.fkOffice) : dest.fk_office))
              .ForMember(d => d.fk_control_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkControlType) ? Guid.Parse(src.fkControlType) : dest.fk_control_type));
            CreateMap<tbl_device, DeviceResponseDTO>()
              .ForMember(d => d.deviceId, opt => opt.MapFrom(src => src.device_id.ToString()))
              .ForMember(d => d.deviceName, opt => opt.MapFrom(src => src.device_name))
              .ForMember(d => d.controlTypeName, opt => opt.MapFrom(src => src.control_type.control_type_name))
              .ForMember(d => d.fkControlType, opt => opt.MapFrom(src => src.fk_control_type.ToString()))
              .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office.office_name))
              .ForMember(d => d.fkOffice, opt => opt.MapFrom(src => src.fk_office.ToString()))
              .ForMember(d => d.deviceTypeName, opt => opt.MapFrom(src => src.device_type.device_type_name))
              .ForMember(d => d.fkDeviceType, opt => opt.MapFrom(src => src.fk_device_type.ToString()));
        }

    }
}
