namespace EnergyMonitor.DTOs;

// ─── Breadcrumb / Navigation ────────────────────────────────────────────────

public class BreadcrumbDto
{
    public Guid   Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // business|facility|building|floor|section|office|sensor
}

// ─── Shared KPI block (reused at every level) ────────────────────────────────

public class KpiSummaryDto
{
    public double TotalActiveEnergyKwh    { get; set; }
    public double TotalReactiveEnergyKvarh{ get; set; }
    public double AvgActivePowerW         { get; set; }
    public double AvgPowerFactor          { get; set; }
    public double AvgVoltage              { get; set; }
    public double AvgCurrent             { get; set; }
    public double AvgFrequency            { get; set; }
    public double PeakActivePowerW        { get; set; }
    public int    SensorCount             { get; set; }
    public int    AlertCount              { get; set; }  // PF < 0.85 or V outside 210–250
}

// ─── Business level ──────────────────────────────────────────────────────────

public class BusinessDashboardDto
{
    public Guid              BusinessId    { get; set; }
    public string            BusinessName  { get; set; } = string.Empty;
    public KpiSummaryDto     Kpis          { get; set; } = new();
    public List<FacilityCardDto> Facilities { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new(); // 24-h kWh sum
}

public class FacilityCardDto
{
    public Guid   FacilityId          { get; set; }
    public string FacilityName        { get; set; } = string.Empty;
    public double TotalActiveEnergyKwh{ get; set; }
    public double AvgPowerFactor      { get; set; }
    public int    SensorCount         { get; set; }
    public int    AlertCount          { get; set; }
}

// ─── Facility level ──────────────────────────────────────────────────────────

public class FacilityDashboardDto
{
    public Guid              FacilityId   { get; set; }
    public string            FacilityName { get; set; } = string.Empty;
    public KpiSummaryDto     Kpis         { get; set; } = new();
    public List<BuildingCardDto> Buildings { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new();
}

public class BuildingCardDto
{
    public Guid   BuildingId           { get; set; }
    public string BuildingName         { get; set; } = string.Empty;
    public double TotalActiveEnergyKwh { get; set; }
    public double AvgPowerFactor       { get; set; }
    public int    SensorCount          { get; set; }
    public int    AlertCount           { get; set; }
}

// ─── Building level ──────────────────────────────────────────────────────────

public class BuildingDashboardDto
{
    public Guid             BuildingId   { get; set; }
    public string           BuildingName { get; set; } = string.Empty;
    public KpiSummaryDto    Kpis         { get; set; } = new();
    public List<FloorCardDto> Floors     { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new();
}

public class FloorCardDto
{
    public Guid   FloorId               { get; set; }
    public string FloorName             { get; set; } = string.Empty;
    public double TotalActiveEnergyKwh  { get; set; }
    public double AvgPowerFactor        { get; set; }
    public int    SensorCount           { get; set; }
    public int    AlertCount            { get; set; }
}

// ─── Floor level ─────────────────────────────────────────────────────────────

public class FloorDashboardDto
{
    public Guid             FloorId    { get; set; }
    public string           FloorName  { get; set; } = string.Empty;
    public KpiSummaryDto    Kpis       { get; set; } = new();
    public List<SectionCardDto> Sections { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new();
}

public class SectionCardDto
{
    public Guid   SectionId             { get; set; }
    public string SectionName           { get; set; } = string.Empty;
    public double TotalActiveEnergyKwh  { get; set; }
    public double AvgPowerFactor        { get; set; }
    public int    SensorCount           { get; set; }
    public int    AlertCount            { get; set; }
}

// ─── Section level ───────────────────────────────────────────────────────────

public class SectionDashboardDto
{
    public Guid             SectionId   { get; set; }
    public string           SectionName { get; set; } = string.Empty;
    public KpiSummaryDto    Kpis        { get; set; } = new();
    public List<OfficeCardDto> Offices  { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new();
}

public class OfficeCardDto
{
    public Guid   OfficeId              { get; set; }
    public string OfficeName            { get; set; } = string.Empty;
    public double TotalActiveEnergyKwh  { get; set; }
    public double AvgPowerFactor        { get; set; }
    public int    SensorCount           { get; set; }
    public int    AlertCount            { get; set; }
}

// ─── Office level ────────────────────────────────────────────────────────────

public class OfficeDashboardDto
{
    public Guid             OfficeId    { get; set; }
    public string           OfficeName  { get; set; } = string.Empty;
    public KpiSummaryDto    Kpis        { get; set; } = new();
    public List<SensorCardDto> Sensors  { get; set; } = new();
    public List<TimeSeriesPointDto> HourlyEnergy { get; set; } = new();
}

public class SensorCardDto
{
    public Guid   SensorId              { get; set; }
    public string SensorName            { get; set; } = string.Empty;
    public double LatestVoltage         { get; set; }
    public double LatestCurrent         { get; set; }
    public double LatestActivePower     { get; set; }
    public double LatestPowerFactor     { get; set; }
    public double TotalActiveEnergyKwh  { get; set; }
    public bool   HasAlert              { get; set; }
}

// ─── Sensor level (full detail) ──────────────────────────────────────────────

public class SensorDashboardDto
{
    public Guid             SensorId    { get; set; }
    public string           SensorName  { get; set; } = string.Empty;
    public KpiSummaryDto    Kpis        { get; set; } = new();

    // Time-series for charts
    public List<TimeSeriesPointDto> Voltage        { get; set; } = new();
    public List<TimeSeriesPointDto> Current        { get; set; } = new();
    public List<TimeSeriesPointDto> ActivePower    { get; set; } = new();
    public List<TimeSeriesPointDto> ReactivePower  { get; set; } = new();
    public List<TimeSeriesPointDto> ApparentPower  { get; set; } = new();
    public List<TimeSeriesPointDto> PowerFactor    { get; set; } = new();
    public List<TimeSeriesPointDto> Frequency      { get; set; } = new();
    public List<TimeSeriesPointDto> ActiveEnergy   { get; set; } = new();
    public List<TimeSeriesPointDto> ReactiveEnergy { get; set; } = new();

    // PF distribution for donut chart
    public PfDistributionDto PfDistribution { get; set; } = new();

    // Hourly demand heatmap
    public List<HourlyDemandDto> HourlyDemand { get; set; } = new();

    // Raw recent packets for data table
    public List<RawReadingDto> RecentReadings { get; set; } = new();

    // Anomaly alerts
    public List<AlertDto> Alerts { get; set; } = new();
}

// ─── Shared primitives ───────────────────────────────────────────────────────

public class TimeSeriesPointDto
{
    public DateTime Timestamp { get; set; }
    public double   Value     { get; set; }
}

public class PfDistributionDto
{
    public double ExcellentPct   { get; set; } // >= 0.95
    public double GoodPct        { get; set; } // 0.90–0.95
    public double AcceptablePct  { get; set; } // 0.85–0.90
    public double PoorPct        { get; set; } // < 0.85
}

public class HourlyDemandDto
{
    public int    Hour         { get; set; }  // 0–23
    public double AvgActivePowerW { get; set; }
}

public class RawReadingDto
{
    public int      PacketId       { get; set; }
    public DateTime CreatedAt      { get; set; }
    public double   Volt           { get; set; }
    public double   Current        { get; set; }
    public double   ActivePower    { get; set; }
    public double   ReactivePower  { get; set; }
    public double   ApparentPower  { get; set; }
    public double   PowerFactor    { get; set; }
    public double   Frequency      { get; set; }
    public double   ActiveEnergy   { get; set; }
    public double   ReactiveEnergy { get; set; }
}

public class AlertDto
{
    public string   Type      { get; set; } = string.Empty; // "danger"|"warning"|"info"
    public string   Message   { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// ─── Query params ────────────────────────────────────────────────────────────

public class DashboardQueryParams
{
    public DateTime? From  { get; set; }
    public DateTime? To    { get; set; }
    public string    Range { get; set; } = "24h"; // "24h"|"7d"|"30d"
}
