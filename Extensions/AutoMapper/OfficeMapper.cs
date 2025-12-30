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
             .ForMember(d => d.fk_section, opt => opt.MapFrom(src => Guid.Parse(src.fkSection)));
            CreateMap<UpdateOfficeDTO, tbl_office>()
              .ForMember(d => d.office_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.officeName) ? src.officeName : dest.office_name))
              .ForMember(d => d.fk_section, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSection) ? Guid.Parse(src.fkSection) : dest.fk_section));
            CreateMap<tbl_office, OfficeResponseDTO>()
              .ForMember(d => d.officeId, opt => opt.MapFrom(src => src.office_id.ToString()))
              .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office_name))
              .ForMember(d => d.sectionName, opt => opt.MapFrom(src => src.section.section_name))
              .ForMember(d => d.fkSection, opt => opt.MapFrom(src => src.fk_section.ToString()));
        }
    }
}
