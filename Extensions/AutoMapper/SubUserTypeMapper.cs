using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserTypeDTOs;
using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SubUserTypeDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class SubUserTypeMapper: Profile
    {
        private readonly OtherServices otherServices = new();
        public SubUserTypeMapper()
        {
            CreateMap<AddSubUserTypeDTO, tbl_sub_user_type>()
              .ForMember(d => d.sub_user_type_name, opt => opt.MapFrom(src => src.subUserTypeName))
              .ForMember(d => d.sub_user_type_level, opt => opt.MapFrom(src => src.subUserTypeLevel))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));
            CreateMap<UpdateSubUserTypeDTO, tbl_sub_user_type>()
              .ForMember(d => d.sub_user_type_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.subUserTypeName) ? src.subUserTypeName : dest.sub_user_type_name))
              .ForMember(d => d.sub_user_type_level, opt => opt.MapFrom((src, dest) => otherServices.Check(src.subUserTypeLevel) ? src.subUserTypeLevel : dest.sub_user_type_level))
              .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) =>  src.isActive))
              .ForMember(d => d.fk_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUserTypeId) ? Guid.Parse(src.fkUserTypeId) : dest.fk_user_type));
            CreateMap<tbl_sub_user_type, SubUserTypeResponseDTO>()
              .ForMember(d => d.subUserTypeId, opt => opt.MapFrom(src => src.sub_user_type_id))
              .ForMember(d => d.subUserTypeName, opt => opt.MapFrom(src => src.sub_user_type_name))
              .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.sub_user_type_level))
              .ForMember(d => d.fkUserTypeId, opt => opt.MapFrom(src => src.fk_user_type))
              .ForMember(d => d.userTypeName, opt => opt.MapFrom(src => src.user_type.user_type_name))
              .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.sub_user_type_level))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
            CreateMap<SubUserTypeHierarchyDTO, tbl_sub_user_type>()
              .ForMember(d => d.sub_user_type_id, opt => opt.MapFrom((src, dest) => dest.sub_user_type_id))
              .ForMember(d => d.sub_user_type_level, opt => opt.MapFrom((src, dest) => otherServices.Check(src.subUserTypeLevel) ? src.subUserTypeLevel : dest.sub_user_type_level));
        }
    }
}
