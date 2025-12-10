using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.DeviceDTOs;
using APIProduct.Models.DTOs.DeviceTypeDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
{
    public class DeviceTypeMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public DeviceTypeMapper()
        {
            CreateMap<AddDeviceTypeDTO, tbl_device_type>()
             .ForMember(d => d.device_type_name, opt => opt.MapFrom(src => src.deviceTypeName));
            CreateMap<UpdateDeviceTypeDTO, tbl_device_type>()
              .ForMember(d => d.device_type_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.deviceTypeName) ? src.deviceTypeName : dest.device_type_name));
            CreateMap<tbl_device_type, DeviceTypeResponseDTO>()
              .ForMember(d => d.deviceTypeId, opt => opt.MapFrom(src => src.device_type_id.ToString()))
              .ForMember(d => d.deviceTypeName, opt => opt.MapFrom(src => src.device_type_name));
        }
    }
}
