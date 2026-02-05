using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.SectionDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class OfficeMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public OfficeMapper()
        {
            CreateMap<AddOfficeDTO, tbl_office>()
             .ForMember(d => d.office_name, opt => opt.MapFrom(src => src.officeName))
              .ForMember(d => d.is_occupied, opt => opt.MapFrom(src => src.isOcuppied))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
             .ForMember(d => d.fk_section, opt => opt.MapFrom(src => Guid.Parse(src.fkSection)));
            CreateMap<UpdateOfficeDTO, tbl_office>()
              .ForMember(d => d.office_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.officeName) ? src.officeName : dest.office_name))
               .ForMember(d => d.is_occupied, opt => opt.MapFrom(src => src.isOcuppied))
              .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now))
              .ForMember(d => d.fk_section, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSection) ? Guid.Parse(src.fkSection) : dest.fk_section));
            CreateMap<tbl_office, OfficeResponseDTO>()
              .ForMember(d => d.officeId, opt => opt.MapFrom(src => src.office_id.ToString()))
              .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office_name))
              .ForMember(d => d.isOcuppied, opt => opt.MapFrom(src => src.is_occupied))
              .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
              .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at))
              .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at))
              .ForMember(d => d.sectionName, opt => opt.MapFrom(src => src.section.section_name))
              .ForMember(d => d.fkSection, opt => opt.MapFrom(src => src.fk_section.ToString()));
        }
    }
}
