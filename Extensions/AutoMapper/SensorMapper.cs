using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SensorDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class SensorMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public SensorMapper()
        {
            CreateMap<AddSensorDTO, tbl_sensor>()
                .ForMember(d => d.sensor_name, opt => opt.MapFrom(src => src.sensorName))
                .ForMember(d => d.sensor_code, opt => opt.MapFrom(src => src.sensorCode))
                .ForMember(d => d.fk_utility, opt => opt.MapFrom(src => Guid.Parse(src.fkUtility)));

            CreateMap<UpdateSensorDTO, tbl_sensor>()
                .ForMember(d => d.sensor_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.sensorName) ? src.sensorName : dest.sensor_name))
                .ForMember(d => d.sensor_code, opt => opt.MapFrom((src, dest) => otherServices.Check(src.sensorCode) ? src.sensorCode : dest.sensor_code))
                .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => src.is_active))
                .ForMember(d => d.fk_utility, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkUtility) ? Guid.Parse(src.fkUtility) : dest.fk_utility));

            CreateMap<tbl_sensor, SensorResponseDTO>()
                .ForMember(d => d.sensorId, opt => opt.MapFrom(src => src.sensor_id.ToString()))
                .ForMember(d => d.sensorName, opt => opt.MapFrom(src => src.sensor_name))
                .ForMember(d => d.sensorCode, opt => opt.MapFrom(src => src.sensor_code))
                .ForMember(d => d.utilityName, opt => opt.MapFrom(src => src.utility.utility_name))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.is_active))
                .ForMember(d => d.fkUtility, opt => opt.MapFrom(src => src.fk_utility.ToString()));
        }
    }
}
