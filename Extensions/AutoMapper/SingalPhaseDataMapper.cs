using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SingalPhaseDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class SingalPhaseDataMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public SingalPhaseDataMapper()
        {
            CreateMap<AddSingalPhaseDataDTO, tbl_singal_phase_data>()
                .ForMember(d => d.packet_id, opt => opt.MapFrom(src => src.packetId))
                .ForMember(d => d.epoch_sec, opt => opt.MapFrom(src => src.epochSec))
                .ForMember(d => d.volt, opt => opt.MapFrom(src => src.volt))
                .ForMember(d => d.current, opt => opt.MapFrom(src => src.current))
                .ForMember(d => d.apperent_power, opt => opt.MapFrom(src => src.apperentPower))
                .ForMember(d => d.active_power, opt => opt.MapFrom(src => src.activePower))
                .ForMember(d => d.reactive_power, opt => opt.MapFrom(src => src.reactivePower))
                .ForMember(d => d.power_factor, opt => opt.MapFrom(src => src.powerFactor))
                .ForMember(d => d.frequency, opt => opt.MapFrom(src => src.frequency))
                .ForMember(d => d.active_energy, opt => opt.MapFrom(src => src.activeEnergy))
                 .ForMember(d => d.active_energy, opt => opt.MapFrom(src => src.activeEnergy))
                 .ForMember(d => d.fk_sensor, opt => opt.MapFrom(src => Guid.Parse(src.fkSensor)))
                .ForMember(d => d.reactive_energy, opt => opt.MapFrom(src => src.reactiveEnergy));

            CreateMap<UpdateSingalPhaseDataDTO, tbl_singal_phase_data>()
                .ForMember(d => d.packet_id, opt => opt.MapFrom((src, dest) => src.packetId != 0 ? src.packetId : dest.packet_id))
                .ForMember(d => d.epoch_sec, opt => opt.MapFrom((src, dest) => src.epochSec != 0 ? src.epochSec : dest.epoch_sec))
                .ForMember(d => d.volt, opt => opt.MapFrom((src, dest) => src.volt != 0 ? src.volt : dest.volt))
                .ForMember(d => d.current, opt => opt.MapFrom((src, dest) => src.current != 0 ? src.current : dest.current))
                .ForMember(d => d.apperent_power, opt => opt.MapFrom((src, dest) => src.apperentPower != 0 ? src.apperentPower : dest.apperent_power))
                .ForMember(d => d.active_power, opt => opt.MapFrom((src, dest) => src.activePower != 0 ? src.activePower : dest.active_power))
                .ForMember(d => d.reactive_power, opt => opt.MapFrom((src, dest) => src.reactivePower != 0 ? src.reactivePower : dest.reactive_power))
                .ForMember(d => d.power_factor, opt => opt.MapFrom((src, dest) => src.powerFactor != 0 ? src.powerFactor : dest.power_factor))
                .ForMember(d => d.frequency, opt => opt.MapFrom((src, dest) => src.frequency != 0 ? src.frequency : dest.frequency))
                .ForMember(d => d.active_energy, opt => opt.MapFrom((src, dest) => src.activeEnergy != 0 ? src.activeEnergy : dest.active_energy))
                .ForMember(d => d.reactive_energy, opt => opt.MapFrom((src, dest) => src.reactiveEnergy != 0 ? src.reactiveEnergy : dest.reactive_energy));

            CreateMap<tbl_singal_phase_data, SingalPhaseDataResponseDTO>()
                .ForMember(d => d.singalPhaseDataId, opt => opt.MapFrom(src => src.singal_phase_data_id.ToString()))
                .ForMember(d => d.packetId, opt => opt.MapFrom(src => src.packet_id))
                .ForMember(d => d.epochSec, opt => opt.MapFrom(src => src.epoch_sec))
                .ForMember(d => d.volt, opt => opt.MapFrom(src => src.volt))
                .ForMember(d => d.current, opt => opt.MapFrom(src => src.current))
                .ForMember(d => d.apperentPower, opt => opt.MapFrom(src => src.apperent_power))
                .ForMember(d => d.activePower, opt => opt.MapFrom(src => src.active_power))
                .ForMember(d => d.reactivePower, opt => opt.MapFrom(src => src.reactive_power))
                .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString()))
                .ForMember(d => d.fkSensor, opt => opt.MapFrom(src => src.fk_sensor.ToString()))
                .ForMember(d => d.sensorName, opt => opt.MapFrom(src => src.sensor.sensor_name))
                .ForMember(d => d.powerFactor, opt => opt.MapFrom(src => src.power_factor))
                .ForMember(d => d.frequency, opt => opt.MapFrom(src => src.frequency))
                .ForMember(d => d.activeEnergy, opt => opt.MapFrom(src => src.active_energy))
                .ForMember(d => d.reactiveEnergy, opt => opt.MapFrom(src => src.reactive_energy));
        }
    }
}
