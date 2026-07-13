namespace EMO.Models.DTOs.OptimizationRuntimeDTOs
{
    public class OptimizationTariffPeriodDTO
    {
        public string PeriodName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = "Peak";
        public string StartTime { get; set; } = "00:00";
        public string EndTime { get; set; } = "00:00";
        public int? DayOfWeek { get; set; }
        public string? SeasonStart { get; set; }
        public string? SeasonEnd { get; set; }
    }

    public class OptimizationRuntimeConfigurationDTO
    {
        public Guid BusinessId { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "PKR";
        public decimal StandardRatePerKwh { get; set; }
        public decimal PeakRatePerKwh { get; set; }
        public decimal OffPeakRatePerKwh { get; set; }
        public bool HasTariffPlan { get; set; }
        public List<OptimizationTariffPeriodDTO> TariffPeriods { get; set; } = new();

        public bool DemandConfigured { get; set; }
        public decimal DemandLimitKw { get; set; }
        public decimal WarningThresholdPercent { get; set; }
        public decimal RecoveryThresholdKw { get; set; }
        public int DemandIntervalMinutes { get; set; }
        public int StabilizationMinutes { get; set; }
        public bool EnablePeakHourControl { get; set; }
        public bool EnableDemandThresholdControl { get; set; }
        public bool SuggestionOnlyMode { get; set; } = true;
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class OptimizationSuggestionItemDTO
    {
        public string ReasonCode { get; set; } = string.Empty;
        public Guid? SensorId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? ApplianceId { get; set; }
        public string Type { get; set; } = "general";
        public string Severity { get; set; } = "info";
        public string Priority { get; set; } = "Low";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string AffectedAppliance { get; set; } = string.Empty;
        public string AffectedUtility { get; set; } = string.Empty;
        public string AffectedOffice { get; set; } = string.Empty;
        public string Confidence { get; set; } = "Medium";
        public double? EstimatedSavingKwh { get; set; }
        public double? EstimatedSavingCost { get; set; }
        public bool ConflictsWithPeakHour { get; set; }
        public bool CanApplyAction { get; set; }
        public DateTime DetectedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddMinutes(2);
    }

    public class OptimizationSuggestionSyncRequestDTO
    {
        public Guid BusinessId { get; set; }
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
        public List<OptimizationSuggestionItemDTO> Suggestions { get; set; } = new();
    }

    public class OptimizationSuggestionSyncResultDTO
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Expired { get; set; }
        public DateTime SyncedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
