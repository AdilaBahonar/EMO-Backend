namespace EMO.Models.DTOs.HistoricalDataDTOs;

public sealed class HistoricalDataQueryDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Interval { get; set; } = "day";
    public string TimeZone { get; set; } = "UTC";
}

public sealed class HistoricalDataResponseDto
{
    public string Level { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Interval { get; set; } = "day";
    public string TimeZone { get; set; } = "UTC";
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public int SensorCount { get; set; }
    public int PointCount { get; set; }
    public List<HistoricalDataPointDto> Points { get; set; } = new();
}

public sealed class HistoricalDataPointDto
{
    public DateTime BucketStartUtc { get; set; }
    public DateTime BucketEndUtc { get; set; }
    public string BucketStartLocal { get; set; } = string.Empty;
    public string BucketEndLocal { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double EnergyKwh { get; set; }
    public double ReactiveEnergyKvarh { get; set; }
    public double AverageActivePowerW { get; set; }
    public double MaximumActivePowerW { get; set; }
    public double AverageVoltageV { get; set; }
    public double AverageCurrentA { get; set; }
    public double AveragePowerFactor { get; set; }
    public double AverageFrequencyHz { get; set; }
    public int SampleCount { get; set; }
    public int ResetCount { get; set; }
    public int RejectedSpikeCount { get; set; }
}
