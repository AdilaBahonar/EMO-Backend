using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
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
                  .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
               .ForMember(d => d.fk_business, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBusiness) ? Guid.Parse(src.fkBusiness) : dest.fk_business))
                 .ForMember(d => d.fk_handler, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkHandler) ? Guid.Parse(src.fkHandler) : dest.fk_handler))
                .ForMember(d => d.fk_sub_user_type, opt => opt.MapFrom(src => Guid.Parse(src.fkSubUserType)))
                .ForMember(d => d.fk_gender, opt => opt.MapFrom(src => src.fkGender))
                 .ForMember(d => d.user_email, opt => opt.MapFrom(src => src.userEmail))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));

            CreateMap<AddBusinessAndAdminDTO, tbl_user>()
            .ForMember(d => d.user_name, opt => opt.MapFrom(src => src.userName))
            .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
            .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
                .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
             .ForMember(d => d.fk_business, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBusiness) ? Guid.Parse(src.fkBusiness) : dest.fk_business))
               .ForMember(d => d.fk_handler, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkHandler) ? Guid.Parse(src.fkHandler) : dest.fk_handler))
              .ForMember(d => d.fk_sub_user_type, opt => opt.MapFrom(src => Guid.Parse(src.fkSubUserType)))
              .ForMember(d => d.fk_gender, opt => opt.MapFrom(src => src.fkGender))
               .ForMember(d => d.user_email, opt => opt.MapFrom(src => src.userEmail))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));

            CreateMap<UpdateUserDTO, tbl_user>()
              .ForMember(d => d.user_id, opt => opt.MapFrom((src, dest) => dest.user_id))
              .ForMember(d => d.user_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userName) ? src.userName : dest.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
              .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
              .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
               .ForMember(d => d.fk_sub_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSubUserType) ? Guid.Parse(src.fkSubUserType) : dest.fk_sub_user_type))
                .ForMember(d => d.fk_business, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBusiness) ? Guid.Parse(src.fkBusiness) : dest.fk_business))
              .ForMember(d => d.fk_gender, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkGender) ? Guid.Parse(src.fkGender) : dest.fk_gender))
              .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => src.isActive));

            CreateMap<UpdateInnerUserDTO, tbl_user>()
             .ForMember(d => d.user_id, opt => opt.MapFrom((src, dest) => dest.user_id))
             .ForMember(d => d.user_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userName) ? src.userName : dest.user_name))
             .ForMember(d => d.name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.name) ? src.name : dest.name))
             .ForMember(d => d.user_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPhoneNo) ? src.userPhoneNo : dest.user_phone_no))
             .ForMember(d => d.user_password, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userPassword) ? otherServices.encodePassword(src.userPassword) : dest.user_password))
             .ForMember(d => d.user_token, opt => opt.MapFrom((src, dest) => otherServices.Check(src.userToken) ? (src.userToken) : dest.user_token))
             .ForMember(d => d.last_activity_at, opt => opt.MapFrom(src => DateTime.Parse(src.lastActivityAt)))
             .ForMember(d => d.fk_sub_user_type, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSubUserType) ? Guid.Parse(src.fkSubUserType) : dest.fk_sub_user_type));

            CreateMap<tbl_user, UserResponseDTO>()
              .ForMember(d => d.userId, opt => opt.MapFrom(src => src.user_id))
              .ForMember(d => d.userName, opt => opt.MapFrom(src => src.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
              .ForMember(d => d.userPhone, opt => opt.MapFrom(src => src.user_phone_no))
              .ForMember(d => d.fkSubUserType, opt => opt.MapFrom(src => src.fk_sub_user_type))
              .ForMember(d => d.subUserTypeName, opt => opt.MapFrom(src => src.sub_user_type.sub_user_type_name))
              .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.sub_user_type.sub_user_type_level))
              .ForMember(d => d.fkGender, opt => opt.MapFrom(src => src.fk_gender))
              .ForMember(d => d.genderName, opt => opt.MapFrom(src => src.gender.gender_name))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
              .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fk_business))
           .ForMember(d => d.fkHandler, opt => opt.MapFrom(src => src.fk_handler))
           .ForMember(d => d.handlerName, opt => opt.MapFrom(src => src.handler.name != null ? src.handler.name : null))
           .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.businesses.business_name != null ? src.businesses.business_name : null))
              .ForMember(d => d.imageBase64, opt => opt.MapFrom(src => src.user_image != null ? src.user_image.imageBase64 : null));
            CreateMap<tbl_user, UserInnerResponseDTO>()
              .ForMember(d => d.userId, opt => opt.MapFrom(src => src.user_id))
              .ForMember(d => d.userName, opt => opt.MapFrom(src => src.user_name))
              .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
              .ForMember(d => d.userPhoneNo, opt => opt.MapFrom(src => src.user_phone_no))
              .ForMember(d => d.userPassword, opt => opt.MapFrom(src => src.user_password))
              .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.user_token))
            .ForMember(d => d.fkSubUserType, opt => opt.MapFrom(src => src.fk_sub_user_type))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
           .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fk_business))
           .ForMember(d => d.fkHandler, opt => opt.MapFrom(src => src.fk_handler))
           .ForMember(d => d.handlerName, opt => opt.MapFrom(src => src.handler.name != null ? src.handler.name : null))
           .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.businesses.business_name!= null? src.businesses.business_name : null ))
              .ForMember(d => d.fkGender, opt => opt.MapFrom(src => src.fk_gender))
              .ForMember(d => d.genderName, opt => opt.MapFrom(src => src.gender.gender_name))
              .ForMember(d => d.imageBase64, opt => opt.MapFrom(src => src.user_image.imageBase64))
              .ForMember(d => d.subUserTypeName, opt => opt.MapFrom(src => src.sub_user_type.sub_user_type_name))
                .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.sub_user_type.user_type.user_type_level))
              .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.sub_user_type.sub_user_type_level));
            CreateMap<UserInnerResponseDTO, UpdateInnerUserDTO>()
             .ForMember(d => d.userId, opt => opt.MapFrom(src => src.userId))
             .ForMember(d => d.userName, opt => opt.MapFrom(src => src.userName))
             .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
             .ForMember(d => d.userPhoneNo, opt => opt.MapFrom(src => src.userPhoneNo))
             .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fkBusiness))
             .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.userTypeLevel))
             .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.subUserTypeLevel))
             .ForMember(d => d.fkHandler, opt => opt.MapFrom(src => src.fkHandler))
             .ForMember(d => d.handlerName, opt => opt.MapFrom(src => src.handlerName))
             .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.businessName))
             .ForMember(d => d.userPassword, opt => opt.MapFrom(src => src.userPassword))
             .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.userToken))
             .ForMember(d => d.fkSubUserType, opt => opt.MapFrom(src => src.fkSubUserType));
            
        }
    }
}
