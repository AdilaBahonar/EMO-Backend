using AutoMapper;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AgreementDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class AgreementMapper : Profile
    {
        private readonly OtherServices otherServices = new();
        public AgreementMapper()
        {
            // Add
            CreateMap<AddAgreementDTO, tbl_agreement>()
                .ForMember(d => d.agreement_name, opt => opt.MapFrom(src => src.agreementName))
                .ForMember(d => d.agreement_description, opt => opt.MapFrom(src => src.agreementDescription))
                .ForMember(d => d.agreement_start_date, opt => opt.MapFrom(src => src.agreementStartDate))
                .ForMember(d => d.agreement_end_date, opt => opt.MapFrom(src => src.agreementEndDate));

            CreateMap<UpdateAgreementDTO, tbl_agreement>()
                .ForMember(d => d.agreement_name,opt => opt.MapFrom((src, dest) =>otherServices.Check(src.agreementName) ? src.agreementName : dest.agreement_name))
                .ForMember(d => d.agreement_description,opt => opt.MapFrom((src, dest) => otherServices.Check(src.agreementDescription) ? src.agreementDescription : dest.agreement_description))
                .ForMember(d => d.agreement_start_date, opt => opt.MapFrom((src, dest) => otherServices.Check(src.agreementStartDate) ? DateTime.Parse(src.agreementStartDate) : dest.agreement_start_date))
                .ForMember(d => d.agreement_end_date, opt => opt.MapFrom((src, dest) => otherServices.Check(src.agreementEndDate) ? DateTime.Parse(src.agreementEndDate) : dest.agreement_end_date))
                .ForMember(d => d.fk_tenant, opt => opt.MapFrom((src, dest) =>otherServices.Check(src.fkTenant) ? Guid.Parse(src.fkTenant) : dest.fk_tenant))
                .ForMember(d => d.fk_office, opt => opt.MapFrom((src, dest) =>otherServices.Check(src.fkOffice) ? Guid.Parse(src.fkOffice) : dest.fk_office))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(_ => DateTime.Now));
            CreateMap<tbl_agreement, AgreementResponseDTO>()
                 .ForMember(d => d.agreementId, opt => opt.MapFrom(src => src.agreement_id.ToString()))
                 .ForMember(d => d.agreementName, opt => opt.MapFrom(src => src.agreement_name))
                 .ForMember(d => d.agreementDescription, opt => opt.MapFrom(src => src.agreement_description))
                 .ForMember(d => d.agreementStartDate, opt => opt.MapFrom(src => src.agreement_start_date.ToString("yyyy-MM-dd")))
                 .ForMember(d => d.agreementEndDate, opt => opt.MapFrom(src => src.agreement_end_date.ToString("yyyy-MM-dd")))
                 .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString()))
                 .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at.ToString()))
                 .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
                 .ForMember(d => d.fkTenant, opt => opt.MapFrom(src => src.fk_tenant.ToString()))
                 .ForMember(d => d.tenantName, opt => opt.MapFrom(src => src.tenant.user_name))
                 .ForMember(d => d.fkOffice, opt => opt.MapFrom(src => src.fk_office.ToString()))
                 .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office.office_name));

        }
    }
}
