namespace EMO.Models.DTOs.RedisRuntimeDTOs
{
    /// <summary>
    /// Rolling sensor evidence calculated by the live dashboard worker.
    /// A summary is usable for optimization decisions only when IsReady is true
    /// and GeneratedAtUtc is still fresh.
    /// </summary>
    public class SensorStableRedisDTO
    {
        public Guid SensorId { get; set; }
        public Guid BusinessId { get; set; }
        public int WindowSeconds { get; set; } = 60;
        public int MinimumSamples { get; set; }
        public int MinimumSpanSeconds { get; set; }
        public int SampleCount { get; set; }
        public double SpanSeconds { get; set; }
        public double CoveragePercent { get; set; }
        public bool IsReady { get; set; }
        public double AveragePowerW { get; set; }
        public double MinimumPowerW { get; set; }
        public double MaximumPowerW { get; set; }
        public double AveragePowerFactor { get; set; }
        public double AverageVoltageV { get; set; }
        public double AverageCurrentA { get; set; }
        public string RelayState { get; set; } = string.Empty;
        public bool RelayEnabled { get; set; }
        public string Mode { get; set; } = string.Empty;
        public string PowerMode { get; set; } = string.Empty;
        public bool IsStandby { get; set; }
        public DateTime? FirstSampleAtUtc { get; set; }
        public DateTime? LastSampleAtUtc { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
    }
}
