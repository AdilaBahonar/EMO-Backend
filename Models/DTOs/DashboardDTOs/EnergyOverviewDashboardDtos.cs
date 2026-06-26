namespace EMO.Models.DTOs.DashboardDTOs
{

    public class EnergyOverviewDashboardDto
    {
        public double MonthlyTargetKwh { get; set; }
        public double CurrentUsageKwh { get; set; }
        public double UsagePercent { get; set; }

        public List<DeviceTypeSummaryDto> DeviceTypeSummaries { get; set; } = new();
        public List<EnergyMeterOverviewDto> EnergyMeters { get; set; } = new();
        public List<MonthlyEnergyDeviceTypeDto> MonthlyDeviceTypeConsumption { get; set; } = new();
        public DeviceTypeDistributionDto DeviceTypeDistribution { get; set; } = new();
        public AlertOverviewDto Alerts { get; set; } = new();
    }

    public class DeviceTypeSummaryDto
    {
        public string DeviceType { get; set; } = string.Empty;
        public double TotalEnergyKwh { get; set; }
        public double AvgVoltage { get; set; }
        public double AvgActivePowerW { get; set; }
        public double ChangePercent { get; set; }
        public List<double> Sparkline { get; set; } = new();
    }

    public class EnergyMeterOverviewDto
    {
        public string MeterName { get; set; } = string.Empty;
        public double Voltage { get; set; }
        public double PowerW { get; set; }
    }

    public class MonthlyEnergyDeviceTypeDto
    {
        public string Month { get; set; } = string.Empty;
        public double Hvac { get; set; }
        public double Lighting { get; set; }
        public double Miscellaneous { get; set; }
        public double Computation { get; set; }
    }

    public class DeviceTypeDistributionDto
    {
        public double HvacPct { get; set; }
        public double LightingPct { get; set; }
        public double MiscellaneousPct { get; set; }
        public double ComputationPct { get; set; }
    }

    public class AlertOverviewDto
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
    }
}
