using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SubTypeDTOs;
using AutoMapper;
using EMO.Extensions;
using EMO.Models.DTOs.SubTypeDTOs.EMO.Models.DTOs.SubTypeDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class SubTypeMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public SubTypeMapper()
        {
            CreateMap<AddSubTypeDTO, tbl_sub_type>()
                .ForMember(d => d.sub_type_name, opt => opt.MapFrom(src => src.subTypeName))
                .ForMember(d => d.sub_type_level, opt => opt.MapFrom(src => src.subTypeLevel))
                .ForMember(d => d.fk_user_type, opt => opt.MapFrom(src => Guid.Parse(src.fkUserType)));

            CreateMap<UpdateSubTypeDTO, tbl_sub_type>()
                .ForMember(d => d.sub_type_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.subTypeName) ? src.subTypeName : dest.sub_type_name))
                .ForMember(d => d.sub_type_level, opt => opt.MapFrom((src, dest) => otherServices.Check(src.subTypeLevel) ? src.subTypeLevel : dest.sub_type_level))
                .ForMember(d => d.fk_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUserType) ? Guid.Parse(src.fkUserType) : dest.fk_user_type));

            CreateMap<tbl_sub_type, SubTypeResponseDTO>()
                .ForMember(d => d.subTypeId, opt => opt.MapFrom(src => src.sub_type_id.ToString()))
                .ForMember(d => d.subTypeName, opt => opt.MapFrom(src => src.sub_type_name))
                .ForMember(d => d.subTypeLevel, opt => opt.MapFrom(src => src.sub_type_level))
                .ForMember(d => d.userTypeName, opt => opt.MapFrom(src => src.user_type.user_type_name))
                .ForMember(d => d.fkUserType, opt => opt.MapFrom(src => src.fk_user_type.ToString()));
        }
    }
}
