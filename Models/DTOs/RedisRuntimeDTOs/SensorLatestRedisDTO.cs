namespace EMO.Models.DTOs.RedisRuntimeDTOs
{
    public class SensorLatestRedisDTO
    {
        public Guid SensorId { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double ActivePower { get; set; }
        public double ReactivePower { get; set; }
        public double ApparentPower { get; set; }
        public double PowerFactor { get; set; }
        public double Frequency { get; set; }
        public double ActiveEnergy { get; set; }
        public double ReactiveEnergy { get; set; }
        public string RelayState { get; set; } = string.Empty;
        public bool RelayEnabled { get; set; }
    }
}
