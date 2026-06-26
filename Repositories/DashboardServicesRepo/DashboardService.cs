//using Microsoft.EntityFrameworkCore;
//using EnergyMonitor.DTOs;
//using EMO.Models.DBModels;
//using EMO.Models.DBModels.DBTables;

//namespace EnergyMonitor.Services;

//public partial class DashboardService
//{
//    private readonly DBUserManagementContext _db;

//    public DashboardService(DBUserManagementContext db) => _db = db;

//    // ─── Helpers ─────────────────────────────────────────────────────────────

//    private static (DateTime from, DateTime to) ResolveRange(DashboardQueryParams q)
//    {
//        var to   = q.To   ?? DateTime.UtcNow;
//        var from = q.From ?? q.Range switch
//        {
//            "7d"  => to.AddDays(-7),
//            "30d" => to.AddDays(-30),
//            _     => to.AddHours(-24)
//        };
//        return (from, to);
//    }

//    private static int AlertCount(IEnumerable<(double pf, double volt)> readings) =>
//        readings.Count(r => r.pf < 0.85 || r.volt is < 210 or > 250);

//    // ─── Reusable: KPIs from a sensor-filtered query ──────────────────────────

//    private async Task<KpiSummaryDto> CalcKpisAsync(
//        IQueryable<tbl_singal_phase_data> q, int sensorCount)
//    {
//        var data = await q
//            .Select(x => new { x.power_factor, x.volt, x.active_power,
//                               x.current, x.frequency,
//                               x.active_energy, x.reactive_energy })
//            .ToListAsync();

//        if (!data.Any()) return new KpiSummaryDto { SensorCount = sensorCount };

//        return new KpiSummaryDto
//        {
//            TotalActiveEnergyKwh     = Math.Round(data.Sum(x => (double)x.active_energy)   / 1000, 2),
//            TotalReactiveEnergyKvarh = Math.Round(data.Sum(x => (double)x.reactive_energy) / 1000, 2),
//            AvgActivePowerW          = Math.Round(data.Average(x => (double)x.active_power), 1),
//            AvgPowerFactor           = Math.Round(data.Average(x => (double)x.power_factor), 3),
//            AvgVoltage               = Math.Round(data.Average(x => (double)x.volt),         1),
//            AvgCurrent               = Math.Round(data.Average(x => (double)x.current),      2),
//            AvgFrequency             = Math.Round(data.Average(x => (double)x.frequency),    2),
//            PeakActivePowerW         = Math.Round(data.Max   (x => (double)x.active_power),  1),
//            SensorCount              = sensorCount,
//            AlertCount               = AlertCount(data.Select(x => ((double)x.power_factor, (double)x.volt)))
//        };
//    }

//    // ─── Reusable: hourly energy time series ─────────────────────────────────

//    private async Task<List<TimeSeriesPointDto>> HourlyEnergyAsync(
//        IQueryable<tbl_singal_phase_data> q, DateTime from, DateTime to)
//    {
//        return await q
//            .Where(x => x.created_at >= from && x.created_at <= to)
//            .GroupBy(x => new { x.created_at.Year, x.created_at.Month,
//                                x.created_at.Day,  x.created_at.Hour })
//            .Select(g => new TimeSeriesPointDto
//            {
//                Timestamp = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0, DateTimeKind.Utc),
//                Value     = Math.Round(g.Sum(x => (double)x.active_energy) / 1000, 3)
//            })
//            .OrderBy(x => x.Timestamp)
//            .ToListAsync();
//    }

//    // ─── Reusable: get sensor IDs by traversal ────────────────────────────────

//    private IQueryable<Guid> SensorsByBusiness(Guid businessId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted && s.device.fk_business == businessId)
//           .Select(s => s.sensor_id);

//    private IQueryable<Guid> SensorsByFacility(Guid facilityId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted &&
//                       s.device.office.section.floor.building.fk_facility == facilityId)
//           .Select(s => s.sensor_id);

//    private IQueryable<Guid> SensorsByBuilding(Guid buildingId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted &&
//                       s.device.office.section.floor.fk_building == buildingId)
//           .Select(s => s.sensor_id);

//    private IQueryable<Guid> SensorsByFloor(Guid floorId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted &&
//                       s.device.office.section.fk_floor == floorId)
//           .Select(s => s.sensor_id);

//    private IQueryable<Guid> SensorsBySection(Guid sectionId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted &&
//                       s.device.office.fk_section == sectionId)
//           .Select(s => s.sensor_id);

//    private IQueryable<Guid> SensorsByOffice(Guid officeId) =>
//        _db.tbl_sensor
//           .Where(s => !s.is_deleted && s.device.fk_office == officeId)
//           .Select(s => s.sensor_id);

//    // ─── Business dashboard ───────────────────────────────────────────────────

//    public async Task<BusinessDashboardDto> GetBusinessAsync(
//        Guid businessId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsByBusiness(businessId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var business = await _db.tbl_business
//            .Where(b => b.business_id == businessId)
//            .Select(b => new { b.business_id, b.business_name })
//            .FirstOrDefaultAsync();

//        var facilities = await _db.tbl_facility
//            .Where(f => f.fk_business == businessId && !f.is_deleted)
//            .Select(f => new { f.facility_id, f.facility_name })
//            .ToListAsync();

//        var facilityCards = new List<FacilityCardDto>();
//        foreach (var f in facilities)
//        {
//            var fSensorIds = SensorsByFacility(f.facility_id);
//            var fData = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && fSensorIds.Contains(x.fk_sensor)
//                             && x.created_at >= from && x.created_at <= to)
//                .Select(x => new { x.power_factor, x.volt, x.active_energy })
//                .ToListAsync();

//            facilityCards.Add(new FacilityCardDto
//            {
//                FacilityId           = f.facility_id,
//                FacilityName         = f.facility_name,
//                TotalActiveEnergyKwh = Math.Round(fData.Sum(x => (double)x.active_energy) / 1000, 2),
//                AvgPowerFactor       = fData.Any() ? Math.Round(fData.Average(x => (double)x.power_factor), 3) : 0,
//                SensorCount          = await fSensorIds.CountAsync(),
//                AlertCount           = AlertCount(fData.Select(x => ((double)x.power_factor, (double)x.volt)))
//            });
//        }

//        return new BusinessDashboardDto
//        {
//            BusinessId   = businessId,
//            BusinessName = business?.business_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Facilities   = facilityCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Facility dashboard ───────────────────────────────────────────────────

//    public async Task<FacilityDashboardDto> GetFacilityAsync(
//        Guid facilityId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsByFacility(facilityId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var facility = await _db.tbl_facility
//            .Where(f => f.facility_id == facilityId)
//            .Select(f => new { f.facility_id, f.facility_name })
//            .FirstOrDefaultAsync();

//        var buildings = await _db.tbl_building
//            .Where(b => b.fk_facility == facilityId && !b.is_deleted)
//            .Select(b => new { b.building_id, b.building_name })
//            .ToListAsync();

//        var buildingCards = new List<BuildingCardDto>();
//        foreach (var b in buildings)
//        {
//            var bSensorIds = SensorsByBuilding(b.building_id);
//            var bData = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && bSensorIds.Contains(x.fk_sensor)
//                             && x.created_at >= from && x.created_at <= to)
//                .Select(x => new { x.power_factor, x.volt, x.active_energy })
//                .ToListAsync();

//            buildingCards.Add(new BuildingCardDto
//            {
//                BuildingId           = b.building_id,
//                BuildingName         = b.building_name,
//                TotalActiveEnergyKwh = Math.Round(bData.Sum(x => (double)x.active_energy) / 1000, 2),
//                AvgPowerFactor       = bData.Any() ? Math.Round(bData.Average(x => (double)x.power_factor), 3) : 0,
//                SensorCount          = await bSensorIds.CountAsync(),
//                AlertCount           = AlertCount(bData.Select(x => ((double)x.power_factor, (double)x.volt)))
//            });
//        }

//        return new FacilityDashboardDto
//        {
//            FacilityId   = facilityId,
//            FacilityName = facility?.facility_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Buildings    = buildingCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Building dashboard ───────────────────────────────────────────────────

//    public async Task<BuildingDashboardDto> GetBuildingAsync(
//        Guid buildingId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsByBuilding(buildingId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var building = await _db.tbl_building
//            .Where(b => b.building_id == buildingId)
//            .Select(b => new { b.building_id, b.building_name })
//            .FirstOrDefaultAsync();

//        var floors = await _db.tbl_floor
//            .Where(f => f.fk_building == buildingId && !f.is_deleted)
//            .Select(f => new { f.floor_id, f.floor_name })
//            .ToListAsync();

//        var floorCards = new List<FloorCardDto>();
//        foreach (var f in floors)
//        {
//            var fSensorIds = SensorsByFloor(f.floor_id);
//            var fData = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && fSensorIds.Contains(x.fk_sensor)
//                             && x.created_at >= from && x.created_at <= to)
//                .Select(x => new { x.power_factor, x.volt, x.active_energy })
//                .ToListAsync();

//            floorCards.Add(new FloorCardDto
//            {
//                FloorId              = f.floor_id,
//                FloorName            = f.floor_name,
//                TotalActiveEnergyKwh = Math.Round(fData.Sum(x => (double)x.active_energy) / 1000, 2),
//                AvgPowerFactor       = fData.Any() ? Math.Round(fData.Average(x => (double)x.power_factor), 3) : 0,
//                SensorCount          = await fSensorIds.CountAsync(),
//                AlertCount           = AlertCount(fData.Select(x => ((double)x.power_factor, (double)x.volt)))
//            });
//        }

//        return new BuildingDashboardDto
//        {
//            BuildingId   = buildingId,
//            BuildingName = building?.building_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Floors       = floorCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Floor dashboard ──────────────────────────────────────────────────────

//    public async Task<FloorDashboardDto> GetFloorAsync(
//        Guid floorId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsByFloor(floorId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var floor = await _db.tbl_floor
//            .Where(f => f.floor_id == floorId)
//            .Select(f => new { f.floor_id, f.floor_name })
//            .FirstOrDefaultAsync();

//        var sections = await _db.tbl_section
//            .Where(s => s.fk_floor == floorId && !s.is_deleted)
//            .Select(s => new { s.section_id, s.section_name })
//            .ToListAsync();

//        var sectionCards = new List<SectionCardDto>();
//        foreach (var s in sections)
//        {
//            var sSensorIds = SensorsBySection(s.section_id);
//            var sData = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && sSensorIds.Contains(x.fk_sensor)
//                             && x.created_at >= from && x.created_at <= to)
//                .Select(x => new { x.power_factor, x.volt, x.active_energy })
//                .ToListAsync();

//            sectionCards.Add(new SectionCardDto
//            {
//                SectionId            = s.section_id,
//                SectionName          = s.section_name,
//                TotalActiveEnergyKwh = Math.Round(sData.Sum(x => (double)x.active_energy) / 1000, 2),
//                AvgPowerFactor       = sData.Any() ? Math.Round(sData.Average(x => (double)x.power_factor), 3) : 0,
//                SensorCount          = await sSensorIds.CountAsync(),
//                AlertCount           = AlertCount(sData.Select(x => ((double)x.power_factor, (double)x.volt)))
//            });
//        }

//        return new FloorDashboardDto
//        {
//            FloorId      = floorId,
//            FloorName    = floor?.floor_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Sections     = sectionCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Section dashboard ────────────────────────────────────────────────────

//    public async Task<SectionDashboardDto> GetSectionAsync(
//        Guid sectionId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsBySection(sectionId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var section = await _db.tbl_section
//            .Where(s => s.section_id == sectionId)
//            .Select(s => new { s.section_id, s.section_name })
//            .FirstOrDefaultAsync();

//        var offices = await _db.tbl_office
//            .Where(o => o.fk_section == sectionId && !o.is_deleted)
//            .Select(o => new { o.office_id, o.office_name })
//            .ToListAsync();

//        var officeCards = new List<OfficeCardDto>();
//        foreach (var o in offices)
//        {
//            var oSensorIds = SensorsByOffice(o.office_id);
//            var oData = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && oSensorIds.Contains(x.fk_sensor)
//                             && x.created_at >= from && x.created_at <= to)
//                .Select(x => new { x.power_factor, x.volt, x.active_energy })
//                .ToListAsync();

//            officeCards.Add(new OfficeCardDto
//            {
//                OfficeId             = o.office_id,
//                OfficeName           = o.office_name,
//                TotalActiveEnergyKwh = Math.Round(oData.Sum(x => (double)x.active_energy) / 1000, 2),
//                AvgPowerFactor       = oData.Any() ? Math.Round(oData.Average(x => (double)x.power_factor), 3) : 0,
//                SensorCount          = await oSensorIds.CountAsync(),
//                AlertCount           = AlertCount(oData.Select(x => ((double)x.power_factor, (double)x.volt)))
//            });
//        }

//        return new SectionDashboardDto
//        {
//            SectionId    = sectionId,
//            SectionName  = section?.section_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Offices      = officeCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Office dashboard ─────────────────────────────────────────────────────

//    public async Task<OfficeDashboardDto> GetOfficeAsync(
//        Guid officeId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);
//        var sensorIds  = SensorsByOffice(officeId);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
//                        && x.created_at >= from && x.created_at <= to);

//        var office = await _db.tbl_office
//            .Where(o => o.office_id == officeId)
//            .Select(o => new { o.office_id, o.office_name })
//            .FirstOrDefaultAsync();

//        var sensors = await _db.tbl_sensor
//            .Where(s => s.fk_device == s.device.device_id &&
//                        s.device.fk_office == officeId && !s.is_deleted)
//            .Select(s => new { s.sensor_id, s.sensor_name })
//            .ToListAsync();

//        var sensorCards = new List<SensorCardDto>();
//        foreach (var s in sensors)
//        {
//            var latest = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && x.fk_sensor == s.sensor_id)
//                .OrderByDescending(x => x.epoch_sec)
//                .FirstOrDefaultAsync();

//            var totalEnergy = await _db.tbl_singal_phase_data
//                .Where(x => !x.is_deleted && x.fk_sensor == s.sensor_id
//                             && x.created_at >= from && x.created_at <= to)
//                .SumAsync(x => (double)x.active_energy);

//            sensorCards.Add(new SensorCardDto
//            {
//                SensorId             = s.sensor_id,
//                SensorName           = s.sensor_name,
//                LatestVoltage        = latest != null ? Math.Round((double)latest.volt,          1) : 0,
//                LatestCurrent        = latest != null ? Math.Round((double)latest.current,       2) : 0,
//                LatestActivePower    = latest != null ? Math.Round((double)latest.active_power,  1) : 0,
//                LatestPowerFactor    = latest != null ? Math.Round((double)latest.power_factor,  3) : 0,
//                TotalActiveEnergyKwh = Math.Round(totalEnergy / 1000, 2),
//                HasAlert             = latest != null && (latest.power_factor < 0.85f ||
//                                       latest.volt is < 210 or > 250)
//            });
//        }

//        return new OfficeDashboardDto
//        {
//            OfficeId     = officeId,
//            OfficeName   = office?.office_name ?? string.Empty,
//            Kpis         = await CalcKpisAsync(data, await sensorIds.CountAsync()),
//            Sensors      = sensorCards,
//            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
//        };
//    }

//    // ─── Sensor dashboard (full detail) ───────────────────────────────────────

//    public async Task<SensorDashboardDto> GetSensorAsync(
//        Guid sensorId, DashboardQueryParams q)
//    {
//        var (from, to) = ResolveRange(q);

//        var data = _db.tbl_singal_phase_data
//            .Where(x => !x.is_deleted && x.fk_sensor == sensorId
//                        && x.created_at >= from && x.created_at <= to)
//            .OrderBy(x => x.epoch_sec);

//        var sensor = await _db.tbl_sensor
//            .Where(s => s.sensor_id == sensorId)
//            .Select(s => new { s.sensor_id, s.sensor_name })
//            .FirstOrDefaultAsync();

//        var raw = await data
//            .Select(x => new
//            {
//                x.packet_id, x.created_at,
//                x.volt, x.current,
//                x.active_power, x.reactive_power, x.apperent_power,
//                x.power_factor, x.frequency,
//                x.active_energy, x.reactive_energy,
//                x.epoch_sec
//            })
//            .ToListAsync();

//        T Series<T>(Func<dynamic, double> selector) where T : class
//            => (T)(object)raw.Select(x => new TimeSeriesPointDto
//            {
//                Timestamp = x.created_at,
//                Value     = Math.Round(selector(x), 3)
//            }).ToList();

//        // Power factor distribution
//        var pfAll   = raw.Select(x => (double)x.power_factor).ToList();
//        var pfTotal = pfAll.Count;
//        var pfDist  = pfTotal == 0 ? new PfDistributionDto() : new PfDistributionDto
//        {
//            ExcellentPct  = Math.Round(pfAll.Count(p => p >= 0.95) * 100.0 / pfTotal, 1),
//            GoodPct       = Math.Round(pfAll.Count(p => p >= 0.90 && p < 0.95) * 100.0 / pfTotal, 1),
//            AcceptablePct = Math.Round(pfAll.Count(p => p >= 0.85 && p < 0.90) * 100.0 / pfTotal, 1),
//            PoorPct       = Math.Round(pfAll.Count(p => p < 0.85) * 100.0 / pfTotal, 1)
//        };

//        // Hourly demand
//        var hourlyDemand = raw
//            .GroupBy(x => x.created_at.Hour)
//            .Select(g => new HourlyDemandDto
//            {
//                Hour              = g.Key,
//                AvgActivePowerW   = Math.Round(g.Average(x => (double)x.active_power), 1)
//            })
//            .OrderBy(h => h.Hour)
//            .ToList();

//        // Alerts
//        var alerts = new List<AlertDto>();
//        foreach (var r in raw)
//        {
//            if ((double)r.volt < 210)
//                alerts.Add(new AlertDto { Type = "danger", Message = $"Voltage sag {r.volt:F1} V (< 210 V)", Timestamp = r.created_at });
//            else if ((double)r.volt > 250)
//                alerts.Add(new AlertDto { Type = "danger", Message = $"Voltage surge {r.volt:F1} V (> 250 V)", Timestamp = r.created_at });
//            if ((double)r.power_factor < 0.85)
//                alerts.Add(new AlertDto { Type = "warning", Message = $"Low power factor {r.power_factor:F3} at {r.created_at:HH:mm}", Timestamp = r.created_at });
//        }
//        alerts = alerts.OrderByDescending(a => a.Timestamp).Take(20).ToList();

//        return new SensorDashboardDto
//        {
//            SensorId       = sensorId,
//            SensorName     = sensor?.sensor_name ?? string.Empty,
//            Kpis           = await CalcKpisAsync(data, 1),
//            Voltage        = Series<List<TimeSeriesPointDto>>(x => (double)x.volt),
//            Current        = Series<List<TimeSeriesPointDto>>(x => (double)x.current),
//            ActivePower    = Series<List<TimeSeriesPointDto>>(x => (double)x.active_power),
//            ReactivePower  = Series<List<TimeSeriesPointDto>>(x => (double)x.reactive_power),
//            ApparentPower  = Series<List<TimeSeriesPointDto>>(x => (double)x.apperent_power),
//            PowerFactor    = Series<List<TimeSeriesPointDto>>(x => (double)x.power_factor),
//            Frequency      = Series<List<TimeSeriesPointDto>>(x => (double)x.frequency),
//            ActiveEnergy   = Series<List<TimeSeriesPointDto>>(x => (double)x.active_energy),
//            ReactiveEnergy = Series<List<TimeSeriesPointDto>>(x => (double)x.reactive_energy),
//            PfDistribution = pfDist,
//            HourlyDemand   = hourlyDemand,
//            RecentReadings = raw.OrderByDescending(x => x.epoch_sec).Take(50)
//                .Select(x => new RawReadingDto
//                {
//                    PacketId      = x.packet_id,
//                    CreatedAt     = x.created_at,
//                    Volt          = Math.Round((double)x.volt,          1),
//                    Current       = Math.Round((double)x.current,       2),
//                    ActivePower   = Math.Round((double)x.active_power,  1),
//                    ReactivePower = Math.Round((double)x.reactive_power,1),
//                    ApparentPower = Math.Round((double)x.apperent_power,1),
//                    PowerFactor   = Math.Round((double)x.power_factor,  3),
//                    Frequency     = Math.Round((double)x.frequency,     2),
//                    ActiveEnergy  = Math.Round((double)x.active_energy, 3),
//                    ReactiveEnergy= Math.Round((double)x.reactive_energy,3)
//                }).ToList(),
//            Alerts = alerts
//        };
//    }
//}
using Microsoft.EntityFrameworkCore;
using EnergyMonitor.DTOs;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;

namespace EnergyMonitor.Services;

public partial class DashboardService
{
    private readonly DBUserManagementContext _db;

    public DashboardService(DBUserManagementContext db) => _db = db;

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static (DateTime from, DateTime to) ResolveRange(DashboardQueryParams q)
    {
        var to = q.To ?? DateTime.UtcNow;
        var from = q.From ?? q.Range switch
        {
            "7d" => to.AddDays(-7),
            "30d" => to.AddDays(-30),
            _ => to.AddHours(-24)
        };

        return (from, to);
    }

    private static int AlertCount(IEnumerable<(double pf, double volt)> readings) =>
        readings.Count(r => r.pf < 0.85 || r.volt is < 210 or > 250);

    // ─── Energy Calculation Helpers ──────────────────────────────────────────
    // active_energy and reactive_energy are cumulative meter readings.
    // Correct consumption = current reading - previous reading, sensor-wise.
    // Negative differences are ignored because they usually mean meter reset.

    private static double CalculateSingleSensorConsumption<T>(
        IEnumerable<T> rows,
        Func<T, DateTime> timeSelector,
        Func<T, double> energySelector)
    {
        var orderedRows = rows
            .OrderBy(timeSelector)
            .ToList();

        if (orderedRows.Count < 2)
            return 0;

        double total = 0;

        for (int i = 1; i < orderedRows.Count; i++)
        {
            var previousEnergy = energySelector(orderedRows[i - 1]);
            var currentEnergy = energySelector(orderedRows[i]);

            var consumption = currentEnergy - previousEnergy;

            if (consumption > 0)
            {
                total += consumption;
            }
        }

        return total;
    }

    private static double CalculateSensorWiseConsumption<T>(
        IEnumerable<T> rows,
        Func<T, Guid> sensorSelector,
        Func<T, DateTime> timeSelector,
        Func<T, double> energySelector)
    {
        return rows
            .GroupBy(sensorSelector)
            .Sum(sensorGroup => CalculateSingleSensorConsumption(sensorGroup, timeSelector, energySelector));
    }

    // ─── Reusable: KPIs from a sensor-filtered query ──────────────────────────

    private async Task<KpiSummaryDto> CalcKpisAsync(
        IQueryable<tbl_singal_phase_data> q, int sensorCount)
    {
        var data = await q
            .Select(x => new
            {
                x.fk_sensor,
                x.created_at,
                x.power_factor,
                x.volt,
                x.active_power,
                x.current,
                x.frequency,
                x.active_energy,
                x.reactive_energy
            })
            .ToListAsync();

        if (!data.Any())
            return new KpiSummaryDto { SensorCount = sensorCount };

        var totalActiveEnergyKwh = CalculateSensorWiseConsumption(
            data,
            x => x.fk_sensor,
            x => x.created_at,
            x => (double)x.active_energy);

        var totalReactiveEnergyKvarh = CalculateSensorWiseConsumption(
            data,
            x => x.fk_sensor,
            x => x.created_at,
            x => (double)x.reactive_energy);

        return new KpiSummaryDto
        {
            TotalActiveEnergyKwh = Math.Round(totalActiveEnergyKwh, 2),
            TotalReactiveEnergyKvarh = Math.Round(totalReactiveEnergyKvarh, 2),
            AvgActivePowerW = Math.Round(data.Average(x => (double)x.active_power), 1),
            AvgPowerFactor = Math.Round(data.Average(x => (double)x.power_factor), 3),
            AvgVoltage = Math.Round(data.Average(x => (double)x.volt), 1),
            AvgCurrent = Math.Round(data.Average(x => (double)x.current), 2),
            AvgFrequency = Math.Round(data.Average(x => (double)x.frequency), 2),
            PeakActivePowerW = Math.Round(data.Max(x => (double)x.active_power), 1),
            SensorCount = sensorCount,
            AlertCount = AlertCount(data.Select(x => ((double)x.power_factor, (double)x.volt)))
        };
    }

    // ─── Reusable: hourly energy time series ─────────────────────────────────

    private async Task<List<TimeSeriesPointDto>> HourlyEnergyAsync(
        IQueryable<tbl_singal_phase_data> q, DateTime from, DateTime to)
    {
        var rows = await q
            .Where(x => x.created_at >= from && x.created_at <= to)
            .Select(x => new
            {
                x.fk_sensor,
                x.created_at,
                x.active_energy
            })
            .ToListAsync();

        var hourlyTotals = new Dictionary<DateTime, double>();

        var sensorGroups = rows.GroupBy(x => x.fk_sensor);

        foreach (var sensorGroup in sensorGroups)
        {
            var orderedRows = sensorGroup
                .OrderBy(x => x.created_at)
                .ToList();

            if (orderedRows.Count < 2)
                continue;

            for (int i = 1; i < orderedRows.Count; i++)
            {
                var previous = orderedRows[i - 1];
                var current = orderedRows[i];

                var consumption = (double)current.active_energy - (double)previous.active_energy;

                if (consumption <= 0)
                    continue;

                var hour = new DateTime(
                    current.created_at.Year,
                    current.created_at.Month,
                    current.created_at.Day,
                    current.created_at.Hour,
                    0,
                    0,
                    DateTimeKind.Utc
                );

                if (!hourlyTotals.ContainsKey(hour))
                {
                    hourlyTotals[hour] = 0;
                }

                hourlyTotals[hour] += consumption;
            }
        }

        return hourlyTotals
            .Select(x => new TimeSeriesPointDto
            {
                Timestamp = x.Key,
                Value = Math.Round(x.Value, 3)
            })
            .OrderBy(x => x.Timestamp)
            .ToList();
    }

    // ─── Reusable: get sensor IDs by traversal ────────────────────────────────

    private IQueryable<Guid> SensorsByBusiness(Guid businessId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted && s.device.fk_business == businessId)
           .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByFacility(Guid facilityId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted &&
                       s.device.office.section.floor.building.fk_facility == facilityId)
           .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByBuilding(Guid buildingId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted &&
                       s.device.office.section.floor.fk_building == buildingId)
           .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByFloor(Guid floorId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted &&
                       s.device.office.section.fk_floor == floorId)
           .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsBySection(Guid sectionId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted &&
                       s.device.office.fk_section == sectionId)
           .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByOffice(Guid officeId) =>
        _db.tbl_sensor
           .Where(s => !s.is_deleted && s.device.fk_office == officeId)
           .Select(s => s.sensor_id);

    // ─── Business dashboard ───────────────────────────────────────────────────

    public async Task<BusinessDashboardDto> GetBusinessAsync(
        Guid businessId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsByBusiness(businessId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var business = await _db.tbl_business
            .Where(b => b.business_id == businessId)
            .Select(b => new { b.business_id, b.business_name })
            .FirstOrDefaultAsync();

        var facilities = await _db.tbl_facility
            .Where(f => f.fk_business == businessId && !f.is_deleted)
            .Select(f => new { f.facility_id, f.facility_name })
            .ToListAsync();

        var facilityCards = new List<FacilityCardDto>();

        foreach (var f in facilities)
        {
            var fSensorIds = SensorsByFacility(f.facility_id);

            var fData = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && fSensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.power_factor,
                    x.volt,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                fData,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            facilityCards.Add(new FacilityCardDto
            {
                FacilityId = f.facility_id,
                FacilityName = f.facility_name,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                AvgPowerFactor = fData.Any() ? Math.Round(fData.Average(x => (double)x.power_factor), 3) : 0,
                SensorCount = await fSensorIds.CountAsync(),
                AlertCount = AlertCount(fData.Select(x => ((double)x.power_factor, (double)x.volt)))
            });
        }

        return new BusinessDashboardDto
        {
            BusinessId = businessId,
            BusinessName = business?.business_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Facilities = facilityCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Facility dashboard ───────────────────────────────────────────────────

    public async Task<FacilityDashboardDto> GetFacilityAsync(
        Guid facilityId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsByFacility(facilityId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var facility = await _db.tbl_facility
            .Where(f => f.facility_id == facilityId)
            .Select(f => new { f.facility_id, f.facility_name })
            .FirstOrDefaultAsync();

        var buildings = await _db.tbl_building
            .Where(b => b.fk_facility == facilityId && !b.is_deleted)
            .Select(b => new { b.building_id, b.building_name })
            .ToListAsync();

        var buildingCards = new List<BuildingCardDto>();

        foreach (var b in buildings)
        {
            var bSensorIds = SensorsByBuilding(b.building_id);

            var bData = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && bSensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.power_factor,
                    x.volt,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                bData,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            buildingCards.Add(new BuildingCardDto
            {
                BuildingId = b.building_id,
                BuildingName = b.building_name,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                AvgPowerFactor = bData.Any() ? Math.Round(bData.Average(x => (double)x.power_factor), 3) : 0,
                SensorCount = await bSensorIds.CountAsync(),
                AlertCount = AlertCount(bData.Select(x => ((double)x.power_factor, (double)x.volt)))
            });
        }

        return new FacilityDashboardDto
        {
            FacilityId = facilityId,
            FacilityName = facility?.facility_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Buildings = buildingCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Building dashboard ───────────────────────────────────────────────────

    public async Task<BuildingDashboardDto> GetBuildingAsync(
        Guid buildingId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsByBuilding(buildingId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var building = await _db.tbl_building
            .Where(b => b.building_id == buildingId)
            .Select(b => new { b.building_id, b.building_name })
            .FirstOrDefaultAsync();

        var floors = await _db.tbl_floor
            .Where(f => f.fk_building == buildingId && !f.is_deleted)
            .Select(f => new { f.floor_id, f.floor_name })
            .ToListAsync();

        var floorCards = new List<FloorCardDto>();

        foreach (var f in floors)
        {
            var fSensorIds = SensorsByFloor(f.floor_id);

            var fData = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && fSensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.power_factor,
                    x.volt,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                fData,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            floorCards.Add(new FloorCardDto
            {
                FloorId = f.floor_id,
                FloorName = f.floor_name,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                AvgPowerFactor = fData.Any() ? Math.Round(fData.Average(x => (double)x.power_factor), 3) : 0,
                SensorCount = await fSensorIds.CountAsync(),
                AlertCount = AlertCount(fData.Select(x => ((double)x.power_factor, (double)x.volt)))
            });
        }

        return new BuildingDashboardDto
        {
            BuildingId = buildingId,
            BuildingName = building?.building_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Floors = floorCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Floor dashboard ──────────────────────────────────────────────────────

    public async Task<FloorDashboardDto> GetFloorAsync(
        Guid floorId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsByFloor(floorId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var floor = await _db.tbl_floor
            .Where(f => f.floor_id == floorId)
            .Select(f => new { f.floor_id, f.floor_name })
            .FirstOrDefaultAsync();

        var sections = await _db.tbl_section
            .Where(s => s.fk_floor == floorId && !s.is_deleted)
            .Select(s => new { s.section_id, s.section_name })
            .ToListAsync();

        var sectionCards = new List<SectionCardDto>();

        foreach (var s in sections)
        {
            var sSensorIds = SensorsBySection(s.section_id);

            var sData = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && sSensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.power_factor,
                    x.volt,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                sData,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            sectionCards.Add(new SectionCardDto
            {
                SectionId = s.section_id,
                SectionName = s.section_name,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                AvgPowerFactor = sData.Any() ? Math.Round(sData.Average(x => (double)x.power_factor), 3) : 0,
                SensorCount = await sSensorIds.CountAsync(),
                AlertCount = AlertCount(sData.Select(x => ((double)x.power_factor, (double)x.volt)))
            });
        }

        return new FloorDashboardDto
        {
            FloorId = floorId,
            FloorName = floor?.floor_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Sections = sectionCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Section dashboard ────────────────────────────────────────────────────

    public async Task<SectionDashboardDto> GetSectionAsync(
        Guid sectionId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsBySection(sectionId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var section = await _db.tbl_section
            .Where(s => s.section_id == sectionId)
            .Select(s => new { s.section_id, s.section_name })
            .FirstOrDefaultAsync();

        var offices = await _db.tbl_office
            .Where(o => o.fk_section == sectionId && !o.is_deleted)
            .Select(o => new { o.office_id, o.office_name })
            .ToListAsync();

        var officeCards = new List<OfficeCardDto>();

        foreach (var o in offices)
        {
            var oSensorIds = SensorsByOffice(o.office_id);

            var oData = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && oSensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.power_factor,
                    x.volt,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                oData,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            officeCards.Add(new OfficeCardDto
            {
                OfficeId = o.office_id,
                OfficeName = o.office_name,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                AvgPowerFactor = oData.Any() ? Math.Round(oData.Average(x => (double)x.power_factor), 3) : 0,
                SensorCount = await oSensorIds.CountAsync(),
                AlertCount = AlertCount(oData.Select(x => ((double)x.power_factor, (double)x.volt)))
            });
        }

        return new SectionDashboardDto
        {
            SectionId = sectionId,
            SectionName = section?.section_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Offices = officeCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Office dashboard ─────────────────────────────────────────────────────

    public async Task<OfficeDashboardDto> GetOfficeAsync(
        Guid officeId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorIds = SensorsByOffice(officeId);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                        && x.created_at >= from && x.created_at <= to);

        var office = await _db.tbl_office
            .Where(o => o.office_id == officeId)
            .Select(o => new { o.office_id, o.office_name })
            .FirstOrDefaultAsync();

        var sensors = await _db.tbl_sensor
            .Where(s => s.fk_device == s.device.device_id &&
                        s.device.fk_office == officeId && !s.is_deleted)
            .Select(s => new { s.sensor_id, s.sensor_name })
            .ToListAsync();

        var sensorCards = new List<SensorCardDto>();

        foreach (var s in sensors)
        {
            var latest = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && x.fk_sensor == s.sensor_id)
                .OrderByDescending(x => x.epoch_sec)
                .FirstOrDefaultAsync();

            var sensorEnergyRows = await _db.tbl_singal_phase_data
                .Where(x => !x.is_deleted && x.fk_sensor == s.sensor_id
                             && x.created_at >= from && x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.active_energy
                })
                .ToListAsync();

            var totalEnergyKwh = CalculateSensorWiseConsumption(
                sensorEnergyRows,
                x => x.fk_sensor,
                x => x.created_at,
                x => (double)x.active_energy);

            sensorCards.Add(new SensorCardDto
            {
                SensorId = s.sensor_id,
                SensorName = s.sensor_name,
                LatestVoltage = latest != null ? Math.Round((double)latest.volt, 1) : 0,
                LatestCurrent = latest != null ? Math.Round((double)latest.current, 2) : 0,
                LatestActivePower = latest != null ? Math.Round((double)latest.active_power, 1) : 0,
                LatestPowerFactor = latest != null ? Math.Round((double)latest.power_factor, 3) : 0,
                TotalActiveEnergyKwh = Math.Round(totalEnergyKwh, 2),
                HasAlert = latest != null && (latest.power_factor < 0.85f ||
                           latest.volt is < 210 or > 250)
            });
        }

        return new OfficeDashboardDto
        {
            OfficeId = officeId,
            OfficeName = office?.office_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, await sensorIds.CountAsync()),
            Sensors = sensorCards,
            HourlyEnergy = await HourlyEnergyAsync(data, from, to)
        };
    }

    // ─── Sensor dashboard full detail ─────────────────────────────────────────

    public async Task<SensorDashboardDto> GetSensorAsync(
        Guid sensorId, DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);

        var data = _db.tbl_singal_phase_data
            .Where(x => !x.is_deleted && x.fk_sensor == sensorId
                        && x.created_at >= from && x.created_at <= to)
            .OrderBy(x => x.epoch_sec);

        var sensor = await _db.tbl_sensor
            .Where(s => s.sensor_id == sensorId)
            .Select(s => new { s.sensor_id, s.sensor_name })
            .FirstOrDefaultAsync();

        var raw = await data
            .Select(x => new
            {
                x.packet_id,
                x.created_at,
                x.volt,
                x.current,
                x.active_power,
                x.reactive_power,
                x.apperent_power,
                x.power_factor,
                x.frequency,
                x.active_energy,
                x.reactive_energy,
                x.epoch_sec
            })
            .ToListAsync();

        T Series<T>(Func<dynamic, double> selector) where T : class
            => (T)(object)raw.Select(x => new TimeSeriesPointDto
            {
                Timestamp = x.created_at,
                Value = Math.Round(selector(x), 3)
            }).ToList();

        var pfAll = raw.Select(x => (double)x.power_factor).ToList();
        var pfTotal = pfAll.Count;

        var pfDist = pfTotal == 0 ? new PfDistributionDto() : new PfDistributionDto
        {
            ExcellentPct = Math.Round(pfAll.Count(p => p >= 0.95) * 100.0 / pfTotal, 1),
            GoodPct = Math.Round(pfAll.Count(p => p >= 0.90 && p < 0.95) * 100.0 / pfTotal, 1),
            AcceptablePct = Math.Round(pfAll.Count(p => p >= 0.85 && p < 0.90) * 100.0 / pfTotal, 1),
            PoorPct = Math.Round(pfAll.Count(p => p < 0.85) * 100.0 / pfTotal, 1)
        };

        var hourlyDemand = raw
            .GroupBy(x => x.created_at.Hour)
            .Select(g => new HourlyDemandDto
            {
                Hour = g.Key,
                AvgActivePowerW = Math.Round(g.Average(x => (double)x.active_power), 1)
            })
            .OrderBy(h => h.Hour)
            .ToList();

        var alerts = new List<AlertDto>();

        foreach (var r in raw)
        {
            if ((double)r.volt < 210)
            {
                alerts.Add(new AlertDto
                {
                    Type = "danger",
                    Message = $"Voltage sag {r.volt:F1} V (< 210 V)",
                    Timestamp = r.created_at
                });
            }
            else if ((double)r.volt > 250)
            {
                alerts.Add(new AlertDto
                {
                    Type = "danger",
                    Message = $"Voltage surge {r.volt:F1} V (> 250 V)",
                    Timestamp = r.created_at
                });
            }

            if ((double)r.power_factor < 0.85)
            {
                alerts.Add(new AlertDto
                {
                    Type = "warning",
                    Message = $"Low power factor {r.power_factor:F3} at {r.created_at:HH:mm}",
                    Timestamp = r.created_at
                });
            }
        }

        alerts = alerts
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToList();

        return new SensorDashboardDto
        {
            SensorId = sensorId,
            SensorName = sensor?.sensor_name ?? string.Empty,
            Kpis = await CalcKpisAsync(data, 1),
            Voltage = Series<List<TimeSeriesPointDto>>(x => (double)x.volt),
            Current = Series<List<TimeSeriesPointDto>>(x => (double)x.current),
            ActivePower = Series<List<TimeSeriesPointDto>>(x => (double)x.active_power),
            ReactivePower = Series<List<TimeSeriesPointDto>>(x => (double)x.reactive_power),
            ApparentPower = Series<List<TimeSeriesPointDto>>(x => (double)x.apperent_power),
            PowerFactor = Series<List<TimeSeriesPointDto>>(x => (double)x.power_factor),
            Frequency = Series<List<TimeSeriesPointDto>>(x => (double)x.frequency),
            ActiveEnergy = Series<List<TimeSeriesPointDto>>(x => (double)x.active_energy),
            ReactiveEnergy = Series<List<TimeSeriesPointDto>>(x => (double)x.reactive_energy),
            PfDistribution = pfDist,
            HourlyDemand = hourlyDemand,
            RecentReadings = raw.OrderByDescending(x => x.epoch_sec).Take(50)
                .Select(x => new RawReadingDto
                {
                    PacketId = x.packet_id,
                    CreatedAt = x.created_at,
                    Volt = Math.Round((double)x.volt, 1),
                    Current = Math.Round((double)x.current, 2),
                    ActivePower = Math.Round((double)x.active_power, 1),
                    ReactivePower = Math.Round((double)x.reactive_power, 1),
                    ApparentPower = Math.Round((double)x.apperent_power, 1),
                    PowerFactor = Math.Round((double)x.power_factor, 3),
                    Frequency = Math.Round((double)x.frequency, 2),
                    ActiveEnergy = Math.Round((double)x.active_energy, 3),
                    ReactiveEnergy = Math.Round((double)x.reactive_energy, 3)
                }).ToList(),
            Alerts = alerts
        };
    }
}
