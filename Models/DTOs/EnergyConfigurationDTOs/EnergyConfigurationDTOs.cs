using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.EnergyConfigurationDTOs
{
    public class TariffTimePeriodDTO
    {
        public string tariffTimePeriodId { get; set; } = string.Empty;
        [Required] public string periodName { get; set; } = "Peak";
        [Required] public string periodType { get; set; } = "Peak";
        [Required] public string startTime { get; set; } = "18:00";
        [Required] public string endTime { get; set; } = "23:00";
        public int? dayOfWeek { get; set; }
        public string? seasonStart { get; set; }
        public string? seasonEnd { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class EnergyTariffPlanDTO
    {
        public string energyTariffPlanId { get; set; } = string.Empty;
        [Required] public string fkBusiness { get; set; } = string.Empty;
        [Required] public string planName { get; set; } = "Default Energy Tariff";
        [Required] public string currency { get; set; } = "PKR";
        [Range(0, double.MaxValue)] public decimal standardRatePerKwh { get; set; }
        [Range(0, double.MaxValue)] public decimal peakRatePerKwh { get; set; }
        [Range(0, double.MaxValue)] public decimal offPeakRatePerKwh { get; set; }
        [Range(0, double.MaxValue)] public decimal? demandChargePerKw { get; set; }
        public bool isActive { get; set; } = true;
        public List<TariffTimePeriodDTO> timePeriods { get; set; } = new();
    }

    public class DemandManagementSettingDTO
    {
        public string demandManagementSettingId { get; set; } = string.Empty;
        [Required] public string fkBusiness { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)] public decimal demandLimitKw { get; set; } = 15;
        [Range(1, 100)] public decimal warningThresholdPercent { get; set; } = 90;
        [Range(0, double.MaxValue)] public decimal recoveryThresholdKw { get; set; } = 13.5m;
        [Range(1, 60)] public int demandIntervalMinutes { get; set; } = 15;
        [Range(0, 60)] public int stabilizationMinutes { get; set; } = 5;
        public bool enablePeakHourControl { get; set; }
        public bool enableDemandThresholdControl { get; set; }
        public bool suggestionOnlyMode { get; set; } = true;
        public bool isActive { get; set; } = true;
    }

    public class EnergyConfigurationDTO
    {
        public string fkBusiness { get; set; } = string.Empty;
        public EnergyTariffPlanDTO tariffPlan { get; set; } = new();
        public DemandManagementSettingDTO demandManagement { get; set; } = new();
    }
}
