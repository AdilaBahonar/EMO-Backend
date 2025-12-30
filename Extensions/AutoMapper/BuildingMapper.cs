using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.BuildingDTOs;
using APIProduct.Models.DTOs.BusinessDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
{
    public class BuildingMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public BuildingMapper()
        {
            CreateMap<AddBuildingDTO, tbl_building>()
             .ForMember(d => d.building_name, opt => opt.MapFrom(src => src.buildingName))
             .ForMember(d => d.fk_facility, opt => opt.MapFrom(src => Guid.Parse(src.fkFacility)));
            CreateMap<UpdateBuildingDTO, tbl_building>()
              .ForMember(d => d.building_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.buildingName) ? src.buildingName : dest.building_name))
              .ForMember(d => d.fk_facility, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkFacility) ? Guid.Parse(src.fkFacility) : dest.fk_facility));
            CreateMap<tbl_building, BuildingResponseDTO>()
              .ForMember(d => d.buildingId, opt => opt.MapFrom(src => src.building_id.ToString()))
              .ForMember(d => d.buildingName, opt => opt.MapFrom(src => src.building_name))
              .ForMember(d => d.facilityName, opt => opt.MapFrom(src => src.facility.facility_name))
              .ForMember(d => d.fkFacility, opt => opt.MapFrom(src => src.fk_facility.ToString()));
        }
    }
}
