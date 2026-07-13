namespace EMO.Models.DTOs.RedisRuntimeDTOs
{
    public class ApplianceRedisDTO
    {
        public Guid ApplianceId { get; set; }
        public Guid BusinessId { get; set; }
        public Guid? DefaultApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ModelNumber { get; set; } = string.Empty;

        public Guid UtilityId { get; set; }
        public string UtilityName { get; set; } = string.Empty;

        public double RatedVoltage { get; set; }
        public double MinCurrent { get; set; }
        public double MaxCurrent { get; set; }
        public double MinPower { get; set; }
        public double MaxPower { get; set; }
        public double StandbyPower { get; set; }
        public double NormalPowerFactor { get; set; }
        public string PriorityLevel { get; set; } = "Normal";
        public bool IsCritical { get; set; }

        public bool IsDefault { get; set; }
        public bool IsCustom { get; set; }
        public bool IsActive { get; set; }
        public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
