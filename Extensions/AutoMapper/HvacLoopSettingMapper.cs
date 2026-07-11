using AutoMapper;
using EMO.Extensions;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.HvacLoopSettingDTOs;

namespace EMO.Extensions.AutoMapper
{
    public class HvacLoopSettingMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public HvacLoopSettingMapper()
        {
            CreateMap<AddHvacLoopSettingDTO, tbl_hvac_loop_setting>()
                .ForMember(d => d.fk_sensor, opt => opt.MapFrom(src => Guid.Parse(src.fkSensor)))
                .ForMember(d => d.loop_enabled, opt => opt.MapFrom(src => src.loopEnabled))
                .ForMember(d => d.loop_on_seconds, opt => opt.MapFrom(src => src.loopOnSeconds))
                .ForMember(d => d.loop_off_seconds, opt => opt.MapFrom(src => src.loopOffSeconds))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive));

            CreateMap<UpdateHvacLoopSettingDTO, tbl_hvac_loop_setting>()
                .ForMember(d => d.fk_sensor, opt => opt.MapFrom((src, dest) =>
                    otherServices.Check(src.fkSensor) ? Guid.Parse(src.fkSensor) : dest.fk_sensor))
                .ForMember(d => d.loop_enabled, opt => opt.MapFrom(src => src.loopEnabled))
                .ForMember(d => d.loop_on_seconds, opt => opt.MapFrom(src => src.loopOnSeconds))
                .ForMember(d => d.loop_off_seconds, opt => opt.MapFrom(src => src.loopOffSeconds))
                .ForMember(d => d.is_active, opt => opt.MapFrom(src => src.isActive))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<tbl_hvac_loop_setting, HvacLoopSettingResponseDTO>()
                .ForMember(d => d.hvacLoopSettingId, opt => opt.MapFrom(src => src.hvac_loop_setting_id.ToString()))
                .ForMember(d => d.fkSensor, opt => opt.MapFrom(src => src.fk_sensor.ToString()))
                .ForMember(d => d.sensorName, opt => opt.MapFrom(src => src.sensor != null ? src.sensor.sensor_name : string.Empty))
                .ForMember(d => d.utilityName, opt => opt.MapFrom(src => src.sensor != null && src.sensor.utility != null ? src.sensor.utility.utility_name : string.Empty))
                .ForMember(d => d.loopEnabled, opt => opt.MapFrom(src => src.loop_enabled))
                .ForMember(d => d.loopOnSeconds, opt => opt.MapFrom(src => src.loop_on_seconds))
                .ForMember(d => d.loopOffSeconds, opt => opt.MapFrom(src => src.loop_off_seconds))
                .ForMember(d => d.loopStartedAt, opt => opt.MapFrom(src => src.loop_started_at.HasValue ? src.loop_started_at.Value.ToString("o") : string.Empty))
                .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active))
                .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString("o")))
                .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at.ToString("o")));
        }
    }
}
