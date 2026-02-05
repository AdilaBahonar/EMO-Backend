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
            .ForMember(d => d.mode_bus_address, opt => opt.MapFrom(src => src.modebusAddress))
            .ForMember(d => d.meter_id, opt => opt.MapFrom(src => src.meterId))
            .ForMember(d => d.serial_address, opt => opt.MapFrom(src => src.serialAddress))
            .ForMember(d => d.fk_sensor_type, opt => opt.MapFrom(src => Guid.Parse(src.fkSensortype)))
            .ForMember(d => d.fk_office, opt => opt.MapFrom(src => Guid.Parse(src.fkOffice)))
            .ForMember(d => d.fk_device, opt => opt.MapFrom(src => Guid.Parse(src.fkDevice)))
            .ForMember(d => d.fk_utility, opt => opt.MapFrom(src => Guid.Parse(src.fkutility)));

            CreateMap<UpdateSensorDTO, tbl_sensor>()
                .ForMember(d => d.sensor_id, opt => opt.MapFrom(src => Guid.Parse(src.sensorId)))
                .ForMember(d => d.sensor_name, opt => opt.MapFrom(src => src.sensorName))
                .ForMember(d => d.mode_bus_address, opt => opt.MapFrom(src => src.modebusAddress))
                .ForMember(d => d.meter_id, opt => opt.MapFrom(src => src.meterId))
                .ForMember(d => d.serial_address, opt => opt.MapFrom(src => src.serialAddress))
                .ForMember(d => d.fk_sensor_type, opt => opt.MapFrom(src => Guid.Parse(src.fkSensortype)))
                .ForMember(d => d.fk_office, opt => opt.MapFrom(src => Guid.Parse(src.fkOffice)))
                .ForMember(d => d.fk_device, opt => opt.MapFrom(src => Guid.Parse(src.fkDevice)))
                .ForMember(d => d.fk_utility, opt => opt.MapFrom(src => Guid.Parse(src.fkutility)));

            CreateMap<tbl_sensor, SensorResponseDTO>()
                .ForMember(d => d.sensorId, opt => opt.MapFrom(src => src.sensor_id.ToString()))
                .ForMember(d => d.sensorName, opt => opt.MapFrom(src => src.sensor_name))
                .ForMember(d => d.modebusAddress, opt => opt.MapFrom(src => src.mode_bus_address))
                .ForMember(d => d.meterId, opt => opt.MapFrom(src => src.meter_id))
                .ForMember(d => d.serialAddress, opt => opt.MapFrom(src => src.serial_address))
                .ForMember(d => d.fkSensortype, opt => opt.MapFrom(src => src.fk_sensor_type.ToString()))
                .ForMember(d => d.sensorTypeName, opt => opt.MapFrom(src => src.sensor_type.sensor_type_name))
                .ForMember(d => d.fkOffice, opt => opt.MapFrom(src => src.fk_office.ToString()))
                .ForMember(d => d.officeName, opt => opt.MapFrom(src => src.office.office_name))
                .ForMember(d => d.fkDevice, opt => opt.MapFrom(src => src.fk_device.ToString()))
                .ForMember(d => d.deviceName, opt => opt.MapFrom(src => src.device.device_name))
                .ForMember(d => d.fkutility, opt => opt.MapFrom(src => src.fk_utility.ToString()))
                .ForMember(d => d.utilityName, opt => opt.MapFrom(src => src.utility.utility_name));
        }
    }
}
