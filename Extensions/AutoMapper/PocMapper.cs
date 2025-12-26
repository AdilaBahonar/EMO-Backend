using APIProduct.Models.DBModels.DBTables;
using APIProduct.Models.DTOs.BuildingDTOs;
using APIProduct.Models.DTOs.BusinessDTOs;
using APIProduct.Models.DTOs.PocDTOs;
using AutoMapper;
using P3AHR.Extensions;

namespace APIProduct.Extensions.AutoMapper
{
    public class PocMapper: Profile
    {
        private readonly OtherServices otherServices = new();
        public PocMapper()
        {
            CreateMap<AddPocDTO, tbl_poc>()
             .ForMember(d => d.poc_name, opt => opt.MapFrom(src => src.pocName))
             .ForMember(d => d.poc_email, opt => opt.MapFrom(src => src.pocEmail))
             .ForMember(d => d.poc_phone_no, opt => opt.MapFrom(src => src.pocPhoneNo));

            CreateMap<UpdatePocDTO, tbl_poc>()
              .ForMember(d => d.poc_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.pocName) ? src.pocName : dest.poc_name))
              .ForMember(d => d.poc_email, opt => opt.MapFrom((src, dest) => otherServices.Check(src.pocEmail) ? (src.pocEmail) : dest.poc_email))
              .ForMember(d => d.poc_phone_no, opt => opt.MapFrom((src, dest) => otherServices.Check(src.pocPhoneNo) ? (src.pocPhoneNo) : dest.poc_phone_no));

            CreateMap<tbl_poc, PocResponseDTO>()
              .ForMember(d => d.pocId, opt => opt.MapFrom(src => src.poc_id.ToString()))
              .ForMember(d => d.pocName, opt => opt.MapFrom(src => src.poc_name))
              .ForMember(d => d.pocEmail, opt => opt.MapFrom(src => src.poc_email))
              .ForMember(d => d.pocPhoneNo, opt => opt.MapFrom(src => src.poc_phone_no));
        }
    }
}
    

