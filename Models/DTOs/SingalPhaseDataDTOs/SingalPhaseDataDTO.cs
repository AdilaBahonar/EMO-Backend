namespace EMO.Models.DTOs.SingalPhaseDTOs
{
    public class AddSingalPhaseDataDTO
    {
        public int packetId { get; set; }
        public int epochSec { get; set; }
        public float volt { get; set; }
        public float current { get; set; }
        public float apperentPower { get; set; }
        public float activePower { get; set; }
        public float reactivePower { get; set; }
        public float powerFactor { get; set; }
        public float frequency { get; set; }
        public float activeEnergy { get; set; }
        public float reactiveEnergy { get; set; }
    }

    public class UpdateSingalPhaseDataDTO
    {
        public Guid singalPhaseDataId { get; set; }
        public int packetId { get; set; }
        public int epochSec { get; set; }
        public float volt { get; set; }
        public float current { get; set; }
        public float apperentPower { get; set; }
        public float activePower { get; set; }
        public float reactivePower { get; set; }
        public float powerFactor { get; set; }
        public float frequency { get; set; }
        public float activeEnergy { get; set; }
        public float reactiveEnergy { get; set; }
    }

    public class SingalPhaseDataResponseDTO
    {
        public Guid singalPhaseDataId { get; set; }
        public int packetId { get; set; }
        public int epochSec { get; set; }
        public float volt { get; set; }
        public float current { get; set; }
        public float apperentPower { get; set; }
        public float activePower { get; set; }
        public float reactivePower { get; set; }
        public float powerFactor { get; set; }
        public float frequency { get; set; }
        public float activeEnergy { get; set; }
        public float reactiveEnergy { get; set; }
    }
}
