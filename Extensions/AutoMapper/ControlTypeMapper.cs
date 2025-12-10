using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.ControlTypeDTOs;
using APIProduct.Models.DTOs.DeviceTypeDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
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
