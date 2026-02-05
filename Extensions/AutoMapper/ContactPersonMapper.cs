using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ContactPersonDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class ContactPersonMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public ContactPersonMapper()
        {
            CreateMap<AddContactPersonDTO, tbl_contact_person>()
                .ForMember(d => d.contact_person_name, opt => opt.MapFrom(src => src.contactPersonName))
                .ForMember(d => d.contact_person_phone, opt => opt.MapFrom(src => src.contactPersonPhone))
                .ForMember(d => d.contact_person_email, opt => opt.MapFrom(src => src.contactPersonEmail))
                .ForMember(d => d.fk_tenant, opt => opt.MapFrom(src => Guid.Parse(src.fkTenant)));

            CreateMap<UpdateContactPersonDTO, tbl_contact_person>()
                .ForMember(d => d.contact_person_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonName) ? src.contactPersonName : dest.contact_person_name))
                .ForMember(d => d.contact_person_phone, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonPhone) ? src.contactPersonPhone : dest.contact_person_phone))
                .ForMember(d => d.contact_person_email, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonEmail) ? src.contactPersonEmail : dest.contact_person_email))
                .ForMember(d => d.fk_tenant, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkTenant) ? Guid.Parse(src.fkTenant) : dest.fk_tenant));

            CreateMap<tbl_contact_person, ContactPersonResponseDTO>()
                .ForMember(d => d.contactPersonId, opt => opt.MapFrom(src => src.contact_person_id.ToString()))
                .ForMember(d => d.contactPersonName, opt => opt.MapFrom(src => src.contact_person_name))
                .ForMember(d => d.contactPersonPhone, opt => opt.MapFrom(src => src.contact_person_phone))
                .ForMember(d => d.contactPersonEmail, opt => opt.MapFrom(src => src.contact_person_email))
                .ForMember(d => d.tenantName, opt => opt.MapFrom(src => src.tenant.tenant_name))                
                .ForMember(d => d.fkTenant, opt => opt.MapFrom(src => src.fk_tenant.ToString()));
        }
    }
}
