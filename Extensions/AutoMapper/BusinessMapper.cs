using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.DeviceDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class BusinessMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public BusinessMapper()
        {
            CreateMap<AddBusinessDTO, tbl_business>()
             .ForMember(d => d.business_name, opt => opt.MapFrom(src => src.businessName))
             .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
             .ForMember(d => d.business_contact, opt => opt.MapFrom(src => src.businessContact))
             .ForMember(d => d.business_email, opt => opt.MapFrom(src => (src.businessEmail)));

            CreateMap<AddBusinessAndAdminDTO, tbl_business>()
             .ForMember(d => d.business_name, opt => opt.MapFrom(src => src.businessName))
             .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.businessIsActive))
             .ForMember(d => d.business_contact, opt => opt.MapFrom(src => src.businessContact))
             .ForMember(d => d.business_email, opt => opt.MapFrom(src => (src.businessEmail)));

            CreateMap<UpdateBusinessDTO, tbl_business>()
              .ForMember(d => d.business_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.businessName) ? src.businessName : dest.business_name))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
              .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now))
              .ForMember(d => d.business_email, opt => opt.MapFrom((src, dest) => otherServices.Check(src.businessEmail) ? (src.businessEmail) : dest.business_email))
               .ForMember(d => d.business_contact, opt => opt.MapFrom((src, dest) => otherServices.Check(src.businessContact) ? (src.businessContact) : dest.business_contact));
            CreateMap<tbl_business, BusinessResponseDTO>()
              .ForMember(d => d.businessId, opt => opt.MapFrom(src => src.business_id.ToString()))
              .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.business_name))
              .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.business_id.ToString()))
              .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.business_name))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
              .ForMember(d => d.businessContact, opt => opt.MapFrom(src => src.business_contact))
              .ForMember(d => d.businessEmail, opt => opt.MapFrom(src => src.business_email));
        }
    }
}
