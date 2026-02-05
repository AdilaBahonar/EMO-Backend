using AutoMapper;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.GenderDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class GenderMapper : Profile
    {
        public GenderMapper()
        {
            // Add GenderDTO -> tbl_gender
            CreateMap<AddGenderDTO, tbl_gender>()
                .ForMember(dest => dest.gender_name, opt => opt.MapFrom(src => src.genderName));

            // UpdateGenderDTO -> tbl_gender
            CreateMap<UpdateGenderDTO, tbl_gender>()
                .ForMember(dest => dest.gender_name, opt => opt.MapFrom(src => src.genderName));

            // tbl_gender -> GenderResponseDTO
            CreateMap<tbl_gender, GenderResponseDTO>()
                .ForMember(dest => dest.genderId, opt => opt.MapFrom(src => src.gender_id.ToString()))
                .ForMember(dest => dest.genderName, opt => opt.MapFrom(src => src.gender_name));
        }
    }
}
