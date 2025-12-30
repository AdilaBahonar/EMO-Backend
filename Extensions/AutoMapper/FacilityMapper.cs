using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.FacilityDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class FacilityMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public FacilityMapper()
        {
            CreateMap<AddFacilityDTO, tbl_facility>()
             .ForMember(d => d.facility_name, opt => opt.MapFrom(src => src.facilityName))
             .ForMember(d => d.fk_business, opt => opt.MapFrom(src => Guid.Parse(src.fkBusiness)));
            CreateMap<UpdateFacilityDTO, tbl_facility>()
              .ForMember(d => d.facility_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.facilityName) ? src.facilityName : dest.facility_name))
              .ForMember(d => d.fk_business, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBusiness) ? Guid.Parse(src.fkBusiness) : dest.fk_business));
            CreateMap<tbl_facility, FacilityResponseDTO>()
              .ForMember(d => d.facilityId, opt => opt.MapFrom(src => src.facility_id.ToString()))
              .ForMember(d => d.facilityName, opt => opt.MapFrom(src => src.facility_name))
              .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.business.business_name))
              .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fk_business.ToString()));
        }
    }
}
