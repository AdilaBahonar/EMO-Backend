using AutoMapper;
using EMO.Extensions;
using EMO.Models.DTOs.AuthDTOs;
using EMO.Models.DTOs.UserDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class AuthMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public AuthMapper()
        {
            CreateMap<UserInnerResponseDTO, UserLoginResponseDTO>()
           .ForMember(d => d.userId, opt => opt.MapFrom(src => src.userId))
           .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
           .ForMember(d => d.userName, opt => opt.MapFrom(src => src.userName))
           .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.isActive))
           .ForMember(d => d.fkSubUserType, opt => opt.MapFrom(src => src.fkSubUserType))
           .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fkBusiness))
           .ForMember(d => d.subUserTypeLevel, opt => opt.MapFrom(src => src.subUserTypeLevel))
           .ForMember(d => d.userTypeLevel, opt => opt.MapFrom(src => src.userTypeLevel))
           .ForMember(d => d.fkHandler, opt => opt.MapFrom(src => src.fkHandler))
           .ForMember(d => d.handlerName, opt => opt.MapFrom(src => src.handlerName))
           .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.businessName))
           .ForMember(d => d.fkGender, opt => opt.MapFrom(src => src.fkGender))
           .ForMember(d => d.genderName, opt => opt.MapFrom(src => src.genderName))
           .ForMember(d => d.imageBase64, opt => opt.MapFrom(src => src.imageBase64))
           .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.userToken));

        }
    }
}
