// ========================= Mapper =========================
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SensorTypeDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class SensorTypeMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public SensorTypeMapper()
        {
            CreateMap<AddSensorTypeDTO, tbl_sensor_type>()
                .ForMember(d => d.sensor_type_name, opt => opt.MapFrom(src => src.sensorTypeName))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.is_active))
                .ForMember(d => d.is_type, opt => opt.MapFrom(src => src.is_type));

            CreateMap<UpdateSensorTypeDTO, tbl_sensor_type>()
                .ForMember(d => d.sensor_type_name, opt => opt.MapFrom((src, dest) =>
                    otherServices.Check(src.sensorTypeName) ? src.sensorTypeName : dest.sensor_type_name))
                .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => src.is_active))
                .ForMember(d => d.is_type, opt => opt.MapFrom((src, dest) => src.is_type));

            CreateMap<tbl_sensor_type, SensorTypeResponseDTO>()
                .ForMember(d => d.sensorTypeId, opt => opt.MapFrom(src => src.sensor_type_id.ToString()))
                .ForMember(d => d.sensorTypeName, opt => opt.MapFrom(src => src.sensor_type_name))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.is_active))
                .ForMember(d => d.is_type, opt => opt.MapFrom(src => src.is_type));
        }
    }
}
