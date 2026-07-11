namespace EMO.Models.DTOs.OptimizationDashboardDTOs
{
    public class OptimizationDashboardResponseDTO
    {
        public string Level { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;

        public string Range { get; set; } = "24h";
        public bool IsCustomRange { get; set; } = false;
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public string RangeLabel { get; set; } = string.Empty;

        public double TotalEnergyKwh { get; set; }
        public double CurrentLivePowerW { get; set; }
        public double PeakPowerW { get; set; }
        public int SensorCount { get; set; }
        public int ActiveSensorCount { get; set; }
        public int MeterResetCount { get; set; }
        public int IgnoredSpikeCount { get; set; }

        public List<OptimizationSuggestionDTO> Suggestions { get; set; } = new();
        public List<OptimizationAlertDTO> Alerts { get; set; } = new();
        public List<HighConsumerDTO> HighConsumers { get; set; } = new();
        public List<EntityComparisonDTO> Comparisons { get; set; } = new();
        public List<UtilityConsumptionDTO> UtilityBreakdown { get; set; } = new();

        // New sections for CRM dashboard.
        public List<IdleApplianceDTO> IdleAppliances { get; set; } = new();
        public List<FaultyApplianceDTO> FaultyAppliances { get; set; } = new();
        public List<PeakDemandHourDTO> PeakDemandHours { get; set; } = new();
        public PeakDemandSummaryDTO PeakDemandSummary { get; set; } = new();
        public List<LiveSensorOptimizationDTO> LiveSensors { get; set; } = new();
        public List<EntityComparisonDTO> ComparisonChart { get; set; } = new();
    }

    public class OptimizationSuggestionDTO
    {
        public string Severity { get; set; } = "info"; // critical|warning|info|success
        public string Priority { get; set; } = "Low"; // High|Medium|Low
        public string Type { get; set; } = string.Empty; // peak-demand|idle|faulty|high-consumption|loop|comparison|reset
        public string ReasonCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public string UtilityName { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string TimeBucket { get; set; } = string.Empty;
        public double EstimatedSavingKwh { get; set; }
        public double EstimatedSavingCost { get; set; }
        public bool CanApplyAction { get; set; }
        public bool ConflictsWithPeakHour { get; set; }
    }

    public class OptimizationAlertDTO
    {
        public string Severity { get; set; } = "info";
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }

    public class HighConsumerDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string UtilityName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public double EnergyKwh { get; set; }
        public double CurrentPowerW { get; set; }
        public double SharePercent { get; set; }
    }

    public class EntityComparisonDTO
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double EnergyKwh { get; set; }
        public double CurrentPowerW { get; set; }
        public int SensorCount { get; set; }
        public string Suggestion { get; set; } = string.Empty;
    }

    public class UtilityConsumptionDTO
    {
        public string UtilityName { get; set; } = string.Empty;
        public double EnergyKwh { get; set; }
        public double CurrentPowerW { get; set; }
        public double SharePercent { get; set; }
    }

    public class IdleApplianceDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public string UtilityName { get; set; } = string.Empty;
        public double CurrentPowerW { get; set; }
        public double StandbyPowerW { get; set; }
        public double FlexibleStandbyLimitW { get; set; }
        public bool StandbyAutoOff { get; set; }
        public bool CanTurnOff { get; set; }
        public string ActionLabel { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string SerialAddress { get; set; } = string.Empty;
    }

    public class FaultyApplianceDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public string UtilityName { get; set; } = string.Empty;
        public double CurrentPowerW { get; set; }
        public double ExpectedMaxPowerW { get; set; }
        public double PowerFactor { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
    }

    public class PeakDemandHourDTO
    {
        public DateTime HourUtc { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public int HourOfDay { get; set; }
        public double EnergyKwh { get; set; }
        public double AveragePowerW { get; set; }
        public double PeakPowerW { get; set; }
        public int SensorCount { get; set; }
        public bool IsPeakHour { get; set; }
    }

    public class PeakDemandSummaryDTO
    {
        public string PeakHourLabel { get; set; } = string.Empty;
        public double PeakPowerW { get; set; }
        public double PeakEnergyKwh { get; set; }
        public string RecommendedAvoidHours { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class LiveSensorOptimizationDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public string ApplianceName { get; set; } = string.Empty;
        public string UtilityName { get; set; } = string.Empty;
        public double ActivePowerW { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double PowerFactor { get; set; }
        public double Frequency { get; set; }
        public string RelayState { get; set; } = string.Empty;
        public bool RelayEnabled { get; set; }
        public bool HvacLoopEnabled { get; set; }
        public int HvacLoopOnSeconds { get; set; }
        public int HvacLoopOffSeconds { get; set; }
        public DateTime? ReceivedAtUtc { get; set; }
        public bool IsIdle { get; set; }
        public bool IsFaulty { get; set; }
    }

    public class OptimizationQueryParams
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public string Range { get; set; } = "24h";
    }
}
