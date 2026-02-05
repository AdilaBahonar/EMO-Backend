using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.FloorDTOs;
using EMO.Models.DTOs.SectionDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class SectionMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public SectionMapper()
        {
            CreateMap<AddSectionDTO, tbl_section>()
             .ForMember(d => d.section_name, opt => opt.MapFrom(src => src.sectionName))
               .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
             .ForMember(d => d.fk_floor, opt => opt.MapFrom(src => Guid.Parse(src.fkFloor)));
            CreateMap<UpdateSectionDTO, tbl_section>()
              .ForMember(d => d.section_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.sectionName) ? src.sectionName : dest.section_name))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
              .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now))
              .ForMember(d => d.fk_floor, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkFloor) ? Guid.Parse(src.fkFloor) : dest.fk_floor));
            CreateMap<tbl_section, SectionResponseDTO>()
              .ForMember(d => d.sectionId, opt => opt.MapFrom(src => src.section_id.ToString()))
              .ForMember(d => d.sectionName, opt => opt.MapFrom(src => src.section_name))
              .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
              .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at))
              .ForMember(d => d.floorName, opt => opt.MapFrom(src => src.floor.floor_name))
              .ForMember(d => d.fkFloor, opt => opt.MapFrom(src => src.fk_floor.ToString()));
        }
    }
}
