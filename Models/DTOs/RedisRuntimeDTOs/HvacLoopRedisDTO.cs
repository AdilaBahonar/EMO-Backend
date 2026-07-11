namespace EMO.Models.DTOs.RedisRuntimeDTOs
{
    public class HvacLoopRedisDTO
    {
        public Guid HvacLoopSettingId { get; set; }
        public Guid SensorId { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string SerialAddress { get; set; } = string.Empty;

        public bool LoopEnabled { get; set; }
        public int LoopOnSeconds { get; set; }
        public int LoopOffSeconds { get; set; }
        public DateTime? LoopStartedAtUtc { get; set; }
        public bool IsActive { get; set; }
        public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
