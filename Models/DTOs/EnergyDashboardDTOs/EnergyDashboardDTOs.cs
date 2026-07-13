namespace EMO.Models.DTOs.EnergyDashboardDTOs
{
    public class MonthlyDeviceTypeReportResponseDTO
    {
        public string utilityName { get; set; } = string.Empty;
        public float totalKwh { get; set; } = 0;
        public float percentage { get; set; } = 0;
    }

    public class EnergyConsumptionByDeviceTypeResponseDTO
    {
        public string month { get; set; } = string.Empty;
        public int year { get; set; } = 0;
        public string utilityName { get; set; } = string.Empty;
        public float totalKwh { get; set; } = 0;
    }

    public class PeakNonPeakAnalysisResponseDTO
    {
        public string period { get; set; } = string.Empty;
        public float peakKwh { get; set; } = 0;
        public float nonPeakKwh { get; set; } = 0;
        public float totalKwh { get; set; } = 0;
        public float peakPercentage { get; set; } = 0;
        public float nonPeakPercentage { get; set; } = 0;
    }

    public class PeakNonPeakSummaryResponseDTO
    {
        public float totalPeakKwh { get; set; } = 0;
        public float totalNonPeakKwh { get; set; } = 0;
        public float totalKwh { get; set; } = 0;
        public float peakPercentage { get; set; } = 0;
        public float nonPeakPercentage { get; set; } = 0;
        public string peakStartTime { get; set; } = string.Empty;
        public string peakEndTime { get; set; } = string.Empty;
        public List<PeakNonPeakAnalysisResponseDTO> dailyData { get; set; } = new();
    }

    public class CrmDashboardSummaryResponseDTO
    {
        public double totalEnergyKwh { get; set; } = 0;
        public double currentLoadW { get; set; } = 0;
        public double monthlyCost { get; set; } = 0;
        public int onlineSensors { get; set; } = 0;
        public double savingOpportunity { get; set; } = 0;
        public double peakDemandW { get; set; } = 0;
        public string fromDate { get; set; } = string.Empty;
        public string toDate { get; set; } = string.Empty;
    }

    public class CrmLiveUtilityLoadResponseDTO
    {
        public string utilityName { get; set; } = "Unassigned";
        public double currentLoadW { get; set; } = 0;
        public double percentage { get; set; } = 0;
        public int onlineSensors { get; set; } = 0;
    }

    public class CrmLiveConsumerResponseDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string applianceName { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public double currentLoadW { get; set; } = 0;
        public double voltage { get; set; } = 0;
        public double powerFactor { get; set; } = 0;
        public string relayState { get; set; } = string.Empty;
        public string receivedAtUtc { get; set; } = string.Empty;
    }

    public class CrmDashboardLiveOverviewResponseDTO
    {
        public double energyTodayKwh { get; set; } = 0;
        public double? estimatedCostToday { get; set; }
        public bool costConfigured { get; set; } = false;
        public double currentLoadW { get; set; } = 0;
        public double peakDemandTodayW { get; set; } = 0;
        public int totalSensors { get; set; } = 0;
        public int onlineSensors { get; set; } = 0;
        public int delayedSensors { get; set; } = 0;
        public int offlineSensors { get; set; } = 0;
        public int neverConnectedSensors { get; set; } = 0;
        public double averageVoltage { get; set; } = 0;
        public double averagePowerFactor { get; set; } = 0;
        public int assignedSensors { get; set; } = 0;
        public int configuredOptimizationSensors { get; set; } = 0;
        public int optimizationReadinessPercent { get; set; } = 0;
        public bool tariffConfigured { get; set; } = false;
        public bool demandLimitConfigured { get; set; } = false;
        public double? demandLimitKw { get; set; }
        public int onlineThresholdSeconds { get; set; } = 180;
        public int delayedThresholdSeconds { get; set; } = 600;
        public string liveUpdatedAtUtc { get; set; } = string.Empty;
        public string aggregateUpdatedAtUtc { get; set; } = string.Empty;
        public List<CrmLiveUtilityLoadResponseDTO> utilityLoads { get; set; } = new();
        public List<CrmLiveConsumerResponseDTO> topConsumers { get; set; } = new();
    }

    public class CrmDashboardChartPointDTO
    {
        public string label { get; set; } = string.Empty;
        public string period { get; set; } = string.Empty;
        public double value { get; set; } = 0;
        public double peakKwh { get; set; } = 0;
        public double nonPeakKwh { get; set; } = 0;
        public double totalKwh { get; set; } = 0;
        public double demandW { get; set; } = 0;
    }

    public class CrmDashboardChartSeriesDTO
    {
        public string name { get; set; } = string.Empty;
        public List<double> data { get; set; } = new();
    }

    public class CrmDashboardChartResponseDTO
    {
        public string chartType { get; set; } = string.Empty;
        public string range { get; set; } = string.Empty;
        public string fromDate { get; set; } = string.Empty;
        public string toDate { get; set; } = string.Empty;
        public string unit { get; set; } = "kWh";
        public List<string> categories { get; set; } = new();
        public List<CrmDashboardChartSeriesDTO> series { get; set; } = new();
        public List<CrmDashboardChartPointDTO> points { get; set; } = new();
        public double totalKwh { get; set; } = 0;
        public double totalPeakKwh { get; set; } = 0;
        public double totalNonPeakKwh { get; set; } = 0;
        public double peakDemandW { get; set; } = 0;
        public string peakStartTime { get; set; } = string.Empty;
        public string peakEndTime { get; set; } = string.Empty;
    }

    public class CrmDashboardSuggestionResponseDTO
    {
        public string suggestionId { get; set; } = string.Empty;
        public string severity { get; set; } = "info";
        public string type { get; set; } = "general";
        public string title { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public double? estimatedSavingKwh { get; set; }
        public double? estimatedSavingCost { get; set; }
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string applianceId { get; set; } = string.Empty;
        public string applianceName { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string timeBucket { get; set; } = string.Empty;
        public bool canApplyAction { get; set; } = false;
        public bool conflictsWithPeakHour { get; set; } = false;
        public string reasonCode { get; set; } = string.Empty;
    }
}
