using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserTypeDTOs;
using AutoMapper;
using EMO.Extensions;
using EMO.Models.DTOs.UserDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class UserTypeMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public UserTypeMapper()
        {
            CreateMap<AddUserTypeDTO, tbl_user_type>()
              .ForMember(d => d.user_type_name, opt => opt.MapFrom(src => src.userTypeName))
              .ForMember(d => d.user_type_level, opt => opt.MapFrom(src => src.userTypeLevel))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));
            CreateMap<UpdateUserTypeDTO, tbl_user_type>()
              .ForMember(d => d.user_type_id, opt => opt.MapFrom((src, dest) => dest.user_type_id))
              .ForMember(d => d.user_type_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userTypeName) ? src.userTypeName : dest.user_type_name))
              .ForMember(d => d.user_type_level, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userTypeLevel) ? src.userTypeLevel : dest.user_type_level))
              .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => src.isActive));
            CreateMap<tbl_user_type, UserTypeResponseDTO>()
              .ForMember(d => d.userTypeId, opt => opt.MapFrom(src => src.user_type_id))
              .ForMember(d => d.userTypeName, opt => opt.MapFrom(src => src.user_type_name))
              .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.user_type_level))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
            CreateMap<UserTypeHierarchyDTO, tbl_user_type>()
            .ForMember(d => d.user_type_id, opt => opt.MapFrom((src, dest) => dest.user_type_id))
            .ForMember(d => d.user_type_level, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userTypeLevel) ? src.userTypeLevel : dest.user_type_level));
        }
    }
}
