using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ControlTypeDTOs;
using EMO.Models.DTOs.DeviceTypeDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class ControlTypeMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public ControlTypeMapper()
        {
            CreateMap<AddControlTypeDTO, tbl_control_type>()
             .ForMember(d => d.control_type_name, opt => opt.MapFrom(src => src.controlTypeName));
            CreateMap<UpdateControlTypeDTO, tbl_control_type>()
              .ForMember(d => d.control_type_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.controlTypeName) ? src.controlTypeName : dest.control_type_name));
            CreateMap<tbl_control_type, ControlTypeResponseDTO>()
              .ForMember(d => d.controlTypeId, opt => opt.MapFrom(src => src.control_type_id.ToString()))
              .ForMember(d => d.controlTypeName, opt => opt.MapFrom(src => src.control_type_name));
        }
    }
}
