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
           .ForMember(d => d.userName, opt => opt.MapFrom(src => src.userName))
           .ForMember(d => d.name, opt => opt.MapFrom(src => src.name))
           .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.userToken));
           
        }
    }
}
