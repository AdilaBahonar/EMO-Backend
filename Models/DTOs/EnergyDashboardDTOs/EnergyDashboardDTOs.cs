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
        public List<PeakNonPeakAnalysisResponseDTO> dailyData { get; set; } = new();
    }
}