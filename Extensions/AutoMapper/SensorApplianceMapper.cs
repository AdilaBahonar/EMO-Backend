using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SensorApplianceDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class SensorApplianceMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public SensorApplianceMapper()
        {
            CreateMap<AssignSensorApplianceDTO, tbl_sensor_appliance>()
                .ForMember(d => d.fk_sensor, opt => opt.MapFrom(src => Guid.Parse(src.fkSensor)))
                .ForMember(d => d.fk_appliance, opt => opt.MapFrom(src => Guid.Parse(src.fkAppliance)))
                .ForMember(d => d.remarks, opt => opt.MapFrom(src => src.remarks))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));

            CreateMap<UpdateSensorApplianceDTO, tbl_sensor_appliance>()
                .ForMember(d => d.fk_sensor, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkSensor) ? Guid.Parse(src.fkSensor) : dest.fk_sensor))
                .ForMember(d => d.fk_appliance, opt => opt.MapFrom((src, dest) => otherServices.Check(src.fkAppliance) ? Guid.Parse(src.fkAppliance) : dest.fk_appliance))
                .ForMember(d => d.remarks, opt => opt.MapFrom((src, dest) => otherServices.Check(src.remarks) ? src.remarks : dest.remarks))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<tbl_sensor_appliance, SensorApplianceResponseDTO>()
                .ForMember(d => d.sensorApplianceId, opt => opt.MapFrom(src => src.sensor_appliance_id.ToString()))
                .ForMember(d => d.fkSensor, opt => opt.MapFrom(src => src.fk_sensor.ToString()))
                .ForMember(d => d.sensorName, opt => opt.MapFrom(src => src.sensor.sensor_name))
                .ForMember(d => d.fkAppliance, opt => opt.MapFrom(src => src.fk_appliance.ToString()))
                .ForMember(d => d.applianceName, opt => opt.MapFrom(src => src.appliance.appliance_name))
                .ForMember(d => d.companyName, opt => opt.MapFrom(src => src.appliance.company_name))
                .ForMember(d => d.modelNumber, opt => opt.MapFrom(src => src.appliance.model_number))
                .ForMember(d => d.fkBusiness, opt => opt.MapFrom(src => src.sensor.device.fk_business.ToString()))
                .ForMember(d => d.fkUtility, opt => opt.MapFrom(src => src.appliance.fk_utility.ToString()))
                .ForMember(d => d.utilityName, opt => opt.MapFrom(src => src.appliance.utility.utility_name))
                .ForMember(d => d.ratedVoltage, opt => opt.MapFrom(src => src.appliance.rated_voltage))
                .ForMember(d => d.minCurrent, opt => opt.MapFrom(src => src.appliance.min_current))
                .ForMember(d => d.maxCurrent, opt => opt.MapFrom(src => src.appliance.max_current))
                .ForMember(d => d.minPower, opt => opt.MapFrom(src => src.appliance.min_power))
                .ForMember(d => d.maxPower, opt => opt.MapFrom(src => src.appliance.max_power))
                .ForMember(d => d.standbyPower, opt => opt.MapFrom(src => src.appliance.standby_power))
                .ForMember(d => d.normalPowerFactor, opt => opt.MapFrom(src => src.appliance.normal_power_factor))
                .ForMember(d => d.isCritical, opt => opt.MapFrom(src => src.appliance.is_critical))
                .ForMember(d => d.priorityLevel, opt => opt.MapFrom(src => src.appliance.priority_level))
                .ForMember(d => d.remarks, opt => opt.MapFrom(src => src.remarks))
                .ForMember(d => d.assignedAt, opt => opt.MapFrom(src => src.assigned_at.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at.ToString("yyyy-MM-dd HH:mm:ss")))
                .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
        }
    }
}
