using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.FacilityDTOs;
using EMO.Models.DTOs.FloorDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Update.Internal;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class FloorMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public FloorMapper()
        {
            CreateMap<AddFloorDTO, tbl_floor>()
             .ForMember(d => d.floor_name, opt => opt.MapFrom(src => src.floorName))
              .ForMember(d => d.floor_no, opt => opt.MapFrom(src => src.floorNo))
             .ForMember(d => d.fk_building, opt => opt.MapFrom(src => Guid.Parse(src.fkBuilding)));
            CreateMap<UpdateFloorDTO, tbl_floor>()
              .ForMember(d => d.floor_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.floorName) ? src.floorName : dest.floor_name))
               .ForMember(d => d.floor_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.floorNo) ? src.floorNo : dest.floor_no))
              .ForMember(d => d.fk_building, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBuilding) ? Guid.Parse(src.fkBuilding) : dest.fk_building));
            CreateMap<tbl_floor, FloorResponseDTO>()
              .ForMember(d => d.floorId, opt => opt.MapFrom(src => src.floor_id.ToString()))
              .ForMember(d => d.floorName, opt => opt.MapFrom(src => src.floor_name))
              .ForMember(d => d.floorNo, opt => opt.MapFrom(src => src.floor_no))
              .ForMember(d => d.buildingName, opt => opt.MapFrom(src => src.building.building_name))
              .ForMember(d => d.fkBuilding, opt => opt.MapFrom(src => src.fk_building.ToString()));
        }
    }
}
