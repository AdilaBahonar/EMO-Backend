using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BuildingDTOs;
using EMO.Models.DTOs.BusinessDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class BuildingMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public BuildingMapper()
        {
            CreateMap<AddBuildingDTO, tbl_building>()
             .ForMember(d => d.building_name, opt => opt.MapFrom(src => src.buildingName))
             .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
             .ForMember(d => d.fk_facility, opt => opt.MapFrom(src => Guid.Parse(src.fkFacility)));
            CreateMap<UpdateBuildingDTO, tbl_building>()
              .ForMember(d => d.building_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.buildingName) ? src.buildingName : dest.building_name))
               .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
              .ForMember(d => d.fk_facility, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkFacility) ? Guid.Parse(src.fkFacility) : dest.fk_facility));
            CreateMap<tbl_building, BuildingResponseDTO>()
              .ForMember(d => d.buildingId, opt => opt.MapFrom(src => src.building_id.ToString()))
              .ForMember(d => d.buildingName, opt => opt.MapFrom(src => src.building_name))
              .ForMember(d => d.facilityName, opt => opt.MapFrom(src => src.facility.facility_name))
              .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
              .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at))
              .ForMember(d => d.fkFacility, opt => opt.MapFrom(src => src.fk_facility.ToString()));
        }
    }
}
