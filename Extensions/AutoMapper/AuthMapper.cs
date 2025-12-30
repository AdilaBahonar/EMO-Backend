using AutoMapper;
using P3AHR.Extensions;
using P3AHR.Models.DTOs.AuthDTOs;
using P3AHR.Models.DTOs.UserDTOs;

namespace APIProduct.Extensions.AutoMapper
{
    public class AuthMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public AuthMapper()
        {
            CreateMap<UserInnerResponseDTO, UserLoginResponseDTO>()
           .ForMember(d => d.userId, opt => opt.MapFrom(src => src.userId))
           .ForMember(d => d.userName, opt => opt.MapFrom(src => src.userName))
           .ForMember(d => d.userOfficialEmail, opt => opt.MapFrom(src => src.userOfficialEmail))
           .ForMember(d => d.userToken, opt => opt.MapFrom(src => src.userToken));
           
        }
    }
}
