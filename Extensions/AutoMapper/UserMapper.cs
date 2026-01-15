using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class UserMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public UserMapper()
        {
            CreateMap<AddUserDTO, tbl_user>()
              .ForMember(d => d.user_name, opt => opt.MapFrom(src => src.userName))
              .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
              .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
              .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.encodePassword(dest.user_password)))
              .ForMember(d => d.fk_user_type, opt => opt.MapFrom(src => Guid.Parse(src.fkUserType)));
            CreateMap<UpdateUserDTO, tbl_user>()
              .ForMember(d => d.user_id, opt => opt.MapFrom((src, dest) => dest.user_id))
              .ForMember(d => d.user_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userName) ? src.userName : dest.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
              .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
              .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
              .ForMember(d => d.fk_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUserType) ? Guid.Parse(src.fkUserType) : dest.fk_user_type));

            CreateMap<UpdateInnerUserDTO, tbl_user>()
             .ForMember(d => d.user_id, opt => opt.MapFrom((src, dest) => dest.user_id))
             .ForMember(d => d.user_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userName) ? src.userName : dest.user_name))
             .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
             .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
             .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
             .ForMember(d => d.user_token, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userToken) ? (src.userToken) : dest.user_token))
             .ForMember(d => d.fk_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUserType) ? Guid.Parse(src.fkUserType) : dest.fk_user_type));
            CreateMap<tbl_user, UserResponseDTO>()
              .ForMember(d => d.userId, opt => opt.MapFrom(src => src.user_id))
              .ForMember(d => d.userName, opt => opt.MapFrom(src => src.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
              .ForMember(d => d.userPhone, opt => opt.MapFrom(src => src.user_phone_no))
              .ForMember(d => d.fkUserType, opt => opt.MapFrom(src => src.fk_user_type))
              .ForMember(d => d.userTypeName, opt => opt.MapFrom(src => src.user_type.user_type_name))
              .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.user_type.user_type_level));
            CreateMap<tbl_user, UserInnerResponseDTO>()
              .ForMember(d => d.userId, opt => opt.MapFrom(src => src.user_id))
              .ForMember(d => d.userName, opt => opt.MapFrom(src => src.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
              .ForMember(d => d.userPhoneNo, opt => opt.MapFrom(src => src.user_phone_no))
              .ForMember(d => d.userPassword, opt => opt.MapFrom(src => src.user_password))
              .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.user_token))
              .ForMember(d => d.fkUserType, opt => opt.MapFrom(src => src.fk_user_type))
              .ForMember(d => d.userTypeName, opt => opt.MapFrom(src => src.user_type.user_type_name))
              .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.user_type.user_type_level));
            CreateMap<UserInnerResponseDTO, UpdateInnerUserDTO>()
             .ForMember(d => d.userId, opt => opt.MapFrom(src => src.userId))
             .ForMember(d => d.userName, opt => opt.MapFrom(src => src.userName))
             .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
             .ForMember(d => d.userPhoneNo, opt => opt.MapFrom(src => src.userPhoneNo))
             .ForMember(d => d.userPassword, opt => opt.MapFrom(src => src.userPassword))
             .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.userToken))
             .ForMember(d => d.fkUserType, opt => opt.MapFrom(src => src.fkUserType));
            
        }
    }
}
