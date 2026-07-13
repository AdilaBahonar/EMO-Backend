using EMO.Models.DTOs.EnergyDashboardDTOs;

namespace EMO.Models.DTOs.DeepDiveDTOs;

public class DeepDiveQueryDto
{
    public string Range { get; set; } = "24h";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool ForceRefresh { get; set; } = false;
    public string TimeZone { get; set; } = "UTC";
}

public class DeepDiveResponseDto
{
    public string Level { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string ChildLevel { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Currency { get; set; } = "PKR";
    public string Timezone { get; set; } = "UTC";
    public DeepDiveDataStatusDto DataStatus { get; set; } = new();
    public DeepDiveConfigurationStatusDto Configuration { get; set; } = new();
    public DeepDiveFeatureAvailabilityDto Features { get; set; } = new();
    public DeepDiveSummaryDto Summary { get; set; } = new();
    public List<DeepDiveBreadcrumbDto> Breadcrumbs { get; set; } = new();
    public List<DeepDiveChildDto> Children { get; set; } = new();
    public List<DeepDiveTrendPointDto> Trend { get; set; } = new();
    public DeepDivePeakOffPeakDto PeakOffPeak { get; set; } = new();
    public DeepDiveDemandDto Demand { get; set; } = new();
    public List<DeepDiveIssueDto> ActiveIssues { get; set; } = new();
    public List<DeepDiveSuggestionDto> Suggestions { get; set; } = new();
    public DeepDiveCrmChartsDto CrmCharts { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public bool ServedFromAggregate { get; set; }
    public string Insight { get; set; } = string.Empty;
}

public class DeepDiveCrmChartsDto
{
    public CrmDashboardChartResponseDTO EnergyConsumption { get; set; } = new();
    public CrmDashboardChartResponseDTO PeakNonPeak { get; set; } = new();
    public CrmDashboardChartResponseDTO HighDemand { get; set; } = new();
    public CrmDashboardChartResponseDTO UtilityTrend { get; set; } = new();
    public CrmDashboardChartResponseDTO UtilityMix { get; set; } = new();
}

public class DeepDiveDataStatusDto
{
    public bool HasReadings { get; set; }
    public int SensorCount { get; set; }
    public int SensorsWithReadings { get; set; }
    public DateTime? FirstReadingAt { get; set; }
    public DateTime? LastReadingAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DeepDiveConfigurationStatusDto
{
    public bool IsReadyForOptimization { get; set; }
    public bool HasActiveTariffPlan { get; set; }
    public bool HasTariffRates { get; set; }
    public bool HasPeakSchedule { get; set; }
    public bool HasValidTimezone { get; set; }
    public bool HasDemandSettings { get; set; }
    public bool HasDemandLimit { get; set; }
    public bool HasOfficeSchedules { get; set; }
    public bool HasApplianceAssignments { get; set; }
    public bool HasApplianceThresholds { get; set; }
    public int TotalSensors { get; set; }
    public int SensorsWithAppliance { get; set; }
    public int SensorsWithOptimizationProfile { get; set; }
    public double ApplianceCoveragePercent { get; set; }
    public List<DeepDiveConfigurationRequirementDto> Requirements { get; set; } = new();
}

public class DeepDiveConfigurationRequirementDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "missing"; // ready|partial|missing|optional
    public string Route { get; set; } = string.Empty;
}

public class DeepDiveFeatureAvailabilityDto
{
    public bool EnergyAnalysis { get; set; }
    public bool CostAnalysis { get; set; }
    public bool PeakOffPeakAnalysis { get; set; }
    public bool DemandAnalysis { get; set; }
    public bool DemandThresholdAnalysis { get; set; }
    public bool OptimizationSuggestions { get; set; }
    public bool SavingsCostAnalysis { get; set; }
}

public class DeepDiveSummaryDto
{
    public double EnergyKwh { get; set; }
    public double PreviousEnergyKwh { get; set; }
    public double EnergyChangePercent { get; set; }
    public double? EstimatedCost { get; set; }
    public double? PreviousEstimatedCost { get; set; }
    public double? CostChangePercent { get; set; }
    public double PeakDemandKw { get; set; }
    public DateTime? PeakDemandAt { get; set; }
    public double SavingOpportunityKwh { get; set; }
    public double? SavingOpportunityCost { get; set; }
}

public class DeepDiveBreadcrumbDto
{
    public string Level { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DeepDiveChildDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public double EnergyKwh { get; set; }
    public double SharePercent { get; set; }
    public double? EstimatedCost { get; set; }
    public double PreviousEnergyKwh { get; set; }
    public double ChangePercent { get; set; }
    public double PeakDemandKw { get; set; }
    public int SensorCount { get; set; }
    public int OnlineSensorCount { get; set; }
    public int IssueCount { get; set; }
    public string Status { get; set; } = "Normal";
}

public class DeepDiveTrendPointDto
{
    public DateTime Bucket { get; set; }
    public string Label { get; set; } = string.Empty;
    public double EnergyKwh { get; set; }
    public double PreviousEnergyKwh { get; set; }
    public double DemandKw { get; set; }
    public double PreviousDemandKw { get; set; }
    public double? Cost { get; set; }
    public double? PreviousCost { get; set; }
}

public class DeepDivePeakOffPeakDto
{
    public bool IsAvailable { get; set; }
    public string UnavailableReason { get; set; } = string.Empty;
    public double PeakEnergyKwh { get; set; }
    public double OffPeakEnergyKwh { get; set; }
    public double PeakSharePercent { get; set; }
    public double OffPeakSharePercent { get; set; }
    public double? PeakCost { get; set; }
    public double? OffPeakCost { get; set; }
}

public class DeepDiveDemandDto
{
    public bool HasData { get; set; }
    public bool HasConfiguredLimit { get; set; }
    public string UnavailableReason { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; } = 15;
    public double AverageDemandKw { get; set; }
    public double PeakDemandKw { get; set; }
    public DateTime? PeakDemandAt { get; set; }
    public double? DemandLimitKw { get; set; }
    public double? WarningThresholdKw { get; set; }
    public int? BreachCount { get; set; }
    public int? MinutesAboveThreshold { get; set; }
}

public class DeepDiveIssueDto
{
    public Guid SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string ApplianceName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Advisory";
    public string Message { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public DateTime DetectedAt { get; set; }
}

public class DeepDiveSuggestionDto
{
    public string Priority { get; set; } = "Low";
    public string Title { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public double? EstimatedSavingKwh { get; set; }
    public double? EstimatedSavingCost { get; set; }
    public bool CanApplyAction { get; set; }
}
