namespace EMO.Models.DTOs.RedisRuntimeDTOs
{
    /// <summary>
    /// Redis runtime copy of a business demand configuration.
    /// The live worker reads this DTO without querying PostgreSQL.
    /// </summary>
    public class DemandManagementRedisDTO
    {
        public Guid DemandManagementSettingId { get; set; }
        public Guid BusinessId { get; set; }
        public decimal DemandLimitKw { get; set; }
        public decimal WarningThresholdPercent { get; set; }
        public decimal RecoveryThresholdKw { get; set; }
        public int DemandIntervalMinutes { get; set; }
        public int StabilizationMinutes { get; set; }
        public bool EnablePeakHourControl { get; set; }
        public bool EnableDemandThresholdControl { get; set; }
        public bool SuggestionOnlyMode { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
