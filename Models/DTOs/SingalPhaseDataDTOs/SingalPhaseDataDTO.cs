using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SingalPhaseDTOs
{
    public class AddSingalPhaseDataDTO
    {
        public int packetId { get; set; } = 0;
        public int epochSec { get; set; } = 0;
        public float volt { get; set; } = 0;
        public float current { get; set; } = 0;
        public float apperentPower { get; set; } = 0;
        public float activePower { get; set; } = 0;
        public float reactivePower { get; set; } = 0;
        public float powerFactor { get; set; } = 0;
        public float frequency { get; set; } = 0;
        public float activeEnergy { get; set; } = 0;
        public float reactiveEnergy { get; set; } = 0;
        public string fkSensor { get; set; } = string.Empty;

    }

    public class UpdateSingalPhaseDataDTO
    {
        [Required]
        public string singalPhaseDataId { get; set; } = string.Empty;
        public int packetId { get; set; } = 0;
        public int epochSec { get; set; } = 0;
        public float volt { get; set; } = 0;
        public float current { get; set; } = 0;
        public float apperentPower { get; set; } = 0;
        public float activePower { get; set; } = 0;
        public float reactivePower { get; set; } = 0;
        public float powerFactor { get; set; } = 0;
        public float frequency { get; set; } = 0;
        public float activeEnergy { get; set; } = 0;
        public float reactiveEnergy { get; set; } = 0;
    }

    public class SingalPhaseDataResponseDTO
    {
        public string singalPhaseDataId { get; set; } = string.Empty;
        public int packetId { get; set; } = 0;
        public int epochSec { get; set; } = 0;
        public float volt { get; set; } = 0;
        public float current { get; set; } = 0;
        public float apperentPower { get; set; } = 0;
        public float activePower { get; set; } = 0;
        public float reactivePower { get; set; } = 0;
        public float powerFactor { get; set; } = 0;
        public float frequency { get; set; } = 0;
        public float activeEnergy { get; set; } = 0;
        public float reactiveEnergy { get; set; } = 0;
        public string createdAt { get; set; } = string.Empty;
        public string fkSensor { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
    }
}
