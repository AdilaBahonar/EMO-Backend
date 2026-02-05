using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UtilityDTOs;
using AutoMapper;
using EMO.Extensions;
using EMO.Models.DTOs.UtilityDTOs.EMO.Models.DTOs.UtilityDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class UtilityMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public UtilityMapper()
        {
            CreateMap<AddUtilityDTO, tbl_utility>()
                .ForMember(d => d.utility_name, opt => opt.MapFrom(src => src.utilityName))
                 .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));

            CreateMap<UpdateUtilityDTO, tbl_utility>()
                .ForMember(d => d.utility_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.utilityName) ? src.utilityName : dest.utility_name))
                 .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive)); ;

            CreateMap<tbl_utility, UtilityResponseDTO>()
                .ForMember(d => d.utilityId, opt => opt.MapFrom(src => src.utility_id.ToString()))
                .ForMember(d => d.utilityName, opt => opt.MapFrom(src => src.utility_name))
                .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
        }
    }
}
