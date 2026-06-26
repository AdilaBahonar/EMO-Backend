using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OfficeDTOs;

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
                .ForMember(d => d.fk_business, opt => opt.MapFrom(src => Guid.Parse(src.fkBusiness)))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.fk_section, opt => opt.MapFrom(src => Guid.Parse(src.fkSection)))
                .ForMember(d => d.opening_time, opt => opt.MapFrom(src => ParseTimeOrDefault(src.openingTime, new TimeOnly(9, 0))))
                .ForMember(d => d.closing_time, opt => opt.MapFrom(src => ParseTimeOrDefault(src.closingTime, new TimeOnly(18, 0))))
                .ForMember(d => d.working_days, opt => opt.MapFrom(src => otherServices.Check(src.workingDays) ? src.workingDays : "Monday,Tuesday,Wednesday,Thursday,Friday"))
                .ForMember(d => d.is_24_hours, opt => opt.MapFrom(src => src.is24Hours))
                .ForMember(d => d.after_hours_alert_enabled, opt => opt.MapFrom(src => src.afterHoursAlertEnabled));

            CreateMap<UpdateOfficeDTO, tbl_office>()
                .ForMember(d => d.office_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.officeName) ? src.officeName : dest.office_name))
                .ForMember(d => d.is_occupied, opt => opt.MapFrom(src => src.isOcuppied))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.fk_business, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkBusiness) ? Guid.Parse(src.fkBusiness) : dest.fk_business))
                .ForMember(d => d.fk_section, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSection) ? Guid.Parse(src.fkSection) : dest.fk_section))
                .ForMember(d => d.opening_time, opt => opt.MapFrom((src, dest) => otherServices.Check(src.openingTime) ? ParseTimeOrDefault(src.openingTime, dest.opening_time) : dest.opening_time))
                .ForMember(d => d.closing_time, opt => opt.MapFrom((src, dest) => otherServices.Check(src.closingTime) ? ParseTimeOrDefault(src.closingTime, dest.closing_time) : dest.closing_time))
                .ForMember(d => d.working_days, opt => opt.MapFrom((src, dest) => otherServices.Check(src.workingDays) ? src.workingDays : dest.working_days))
                .ForMember(d => d.is_24_hours, opt => opt.MapFrom(src => src.is24Hours))
                .ForMember(d => d.after_hours_alert_enabled, opt => opt.MapFrom(src => src.afterHoursAlertEnabled))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<tbl_office, OfficeResponseDTO>()
                .ForMember(d => d.officeId, opt => opt.MapFrom(src => src.office_id.ToString()))
                .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office_name))
                .ForMember(d => d.isOcuppied, opt => opt.MapFrom(src => src.is_occupied))
                .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
                .ForMember(d => d.businessName, opt => opt.MapFrom(src => src.business != null ? src.business.business_name : string.Empty))
                .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.fk_business.ToString()))
                .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(d => d.sectionName, opt => opt.MapFrom(src => src.section != null ? src.section.section_name : string.Empty))
                .ForMember(d => d.fkSection, opt => opt.MapFrom(src => src.fk_section.ToString()))
                .ForMember(d => d.openingTime, opt => opt.MapFrom(src => src.opening_time.ToString("HH:mm")))
                .ForMember(d => d.closingTime, opt => opt.MapFrom(src => src.closing_time.ToString("HH:mm")))
                .ForMember(d => d.workingDays, opt => opt.MapFrom(src => src.working_days))
                .ForMember(d => d.is24Hours, opt => opt.MapFrom(src => src.is_24_hours))
                .ForMember(d => d.afterHoursAlertEnabled, opt => opt.MapFrom(src => src.after_hours_alert_enabled));
        }

        private static TimeOnly ParseTimeOrDefault(string? value, TimeOnly defaultValue)
        {
            return TimeOnly.TryParse(value, out var parsedTime) ? parsedTime : defaultValue;
        }
    }
}
