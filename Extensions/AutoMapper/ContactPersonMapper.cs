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
                .ForMember(d => d.fk_agreement, opt => opt.MapFrom(src => Guid.Parse(src.fkAgreement)));

            CreateMap<UpdateContactPersonDTO, tbl_contact_person>()
                .ForMember(d => d.contact_person_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonName) ? src.contactPersonName : dest.contact_person_name))
                .ForMember(d => d.contact_person_phone, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonPhone) ? src.contactPersonPhone : dest.contact_person_phone))
                .ForMember(d => d.contact_person_email, opt => opt.MapFrom((src, dest) => otherServices.Check(src.contactPersonEmail) ? src.contactPersonEmail : dest.contact_person_email))
                .ForMember(d => d.fk_agreement, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkAgreement) ? Guid.Parse(src.fkAgreement) : dest.fk_agreement));

            CreateMap<tbl_contact_person, ContactPersonResponseDTO>()
                .ForMember(d => d.contactPersonId, opt => opt.MapFrom(src => src.contact_person_id.ToString()))
                .ForMember(d => d.contactPersonName, opt => opt.MapFrom(src => src.contact_person_name))
                .ForMember(d => d.contactPersonPhone, opt => opt.MapFrom(src => src.contact_person_phone))
                .ForMember(d => d.contactPersonEmail, opt => opt.MapFrom(src => src.contact_person_email))
                .ForMember(d => d.agreementName, opt => opt.MapFrom(src => src.agreement.agreement_name))                
                .ForMember(d => d.fkAgreement, opt => opt.MapFrom(src => src.fk_agreement.ToString()));
        }
    }
}
