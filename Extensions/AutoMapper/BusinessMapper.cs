using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.BusinessDTOs;
using APIProduct.Models.DTOs.DeviceDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
{
    public class BusinessMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public BusinessMapper()
        {
            CreateMap<AddBusinessDTO, tbl_business>()
             .ForMember(d => d.business_name, opt => opt.MapFrom(src => src.businessName))
             .ForMember(d => d.fk_user, opt => opt.MapFrom(src => Guid.Parse(src.fkUser)));
            CreateMap<UpdateBusinessDTO, tbl_business>()
              .ForMember(d => d.business_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.businessName) ? src.businessName : dest.business_name))
              .ForMember(d => d.fk_user, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUser) ? Guid.Parse(src.fkUser) : dest.fk_user));
            CreateMap<tbl_business, BusinessResponseDTO>()
              .ForMember(d => d.businessId, opt => opt.MapFrom(src => src.business_id.ToString()))
              .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.business_name))
              .ForMember(d => d.userName, opt => opt.MapFrom(src => src.user.user_name))
              .ForMember(d => d.fkUser, opt => opt.MapFrom(src => src.fk_user.ToString()));
        }
    }
}
