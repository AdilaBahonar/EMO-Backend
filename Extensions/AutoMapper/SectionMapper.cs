using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.FloorDTOs;
using APIProduct.Models.DTOs.SectionDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
{
    public class SectionMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public SectionMapper()
        {
            CreateMap<AddSectionDTO, tbl_section>()
             .ForMember(d => d.section_name, opt => opt.MapFrom(src => src.sectionName))
             .ForMember(d => d.fk_floor, opt => opt.MapFrom(src => Guid.Parse(src.fkFloor)));
            CreateMap<UpdateSectionDTO, tbl_section>()
              .ForMember(d => d.section_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.sectionName) ? src.sectionName : dest.section_name))
              .ForMember(d => d.fk_floor, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkFloor) ? Guid.Parse(src.fkFloor) : dest.fk_floor));
            CreateMap<tbl_section, SectionResponseDTO>()
              .ForMember(d => d.sectionId, opt => opt.MapFrom(src => src.section_id.ToString()))
              .ForMember(d => d.sectionName, opt => opt.MapFrom(src => src.section_name))
              .ForMember(d => d.floorName, opt => opt.MapFrom(src => src.floor.floor_name))
              .ForMember(d => d.fkFloor, opt => opt.MapFrom(src => src.fk_floor.ToString()));
        }
    }
}
