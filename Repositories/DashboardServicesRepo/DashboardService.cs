using EMO.Models.DBModels;
using EMO.Repositories.DeepDiveRepo;
using EnergyMonitor.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EnergyMonitor.Services;

public partial class DashboardService
{
    private readonly DBUserManagementContext _db;
    private readonly ISensorEnergyAggregateStore _energyStore;

    public DashboardService(
        DBUserManagementContext db,
        ISensorEnergyAggregateStore energyStore)
    {
        _db = db;
        _energyStore = energyStore;
    }

    // ─── Range and compact aggregate helpers ─────────────────────────────────

    private static (DateTime from, DateTime to) ResolveRange(DashboardQueryParams q)
    {
        var custom = q.From.HasValue || q.To.HasValue;
        var rawTo = EnsureUtc(q.To ?? DateTime.UtcNow);
        var range = (q.Range ?? "24h").Trim().ToLowerInvariant();

        // Keep named ranges stable and aligned with the Deep Dive endpoint/cache.
        var hourlyAnchor = rawTo.Minute < 10 ? rawTo.AddHours(-1) : rawTo;
        var dailyAnchor = rawTo.TimeOfDay < TimeSpan.FromMinutes(10)
            ? rawTo.AddDays(-1)
            : rawTo;

        var to = custom
            ? rawTo
            : range switch
            {
                "1y" or "12m" or "365d" => dailyAnchor.Date,
                "90d" => new DateTime(
                    hourlyAnchor.Year, hourlyAnchor.Month, hourlyAnchor.Day,
                    hourlyAnchor.Hour - hourlyAnchor.Hour % 6, 0, 0, DateTimeKind.Utc),
                _ => new DateTime(
                    hourlyAnchor.Year, hourlyAnchor.Month, hourlyAnchor.Day,
                    hourlyAnchor.Hour, 0, 0, DateTimeKind.Utc)
            };

        var from = q.From.HasValue
            ? EnsureUtc(q.From.Value)
            : range switch
            {
                "7d" => to.AddDays(-7),
                "30d" => to.AddDays(-30),
                "90d" => to.AddDays(-90),
                "1y" or "12m" or "365d" => to.AddYears(-1),
                _ => to.AddHours(-24)
            };

        return (from, to);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private async Task<ScopeData> LoadScopeAsync(
        IQueryable<Guid> sensorQuery,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var sensorIds = await sensorQuery
            .Distinct()
            .ToListAsync(cancellationToken);

        var rows = await _energyStore.LoadAsync(
            sensorIds, from, to, cancellationToken);

        return new ScopeData
        {
            SensorIds = sensorIds,
            Rows = rows
        };
    }

    private static double WeightedAverage(
        IEnumerable<SensorEnergyBucketRow> rows,
        Func<SensorEnergyBucketRow, double> selector)
    {
        double weightedTotal = 0;
        long sampleTotal = 0;

        foreach (var row in rows)
        {
            if (row.SampleCount <= 0)
                continue;

            weightedTotal += selector(row) * row.SampleCount;
            sampleTotal += row.SampleCount;
        }

        return sampleTotal == 0 ? 0 : weightedTotal / sampleTotal;
    }

    private static KpiSummaryDto BuildKpis(
        IReadOnlyCollection<SensorEnergyBucketRow> rows,
        int sensorCount)
    {
        if (rows.Count == 0)
            return new KpiSummaryDto { SensorCount = sensorCount };

        return new KpiSummaryDto
        {
            TotalActiveEnergyKwh = Math.Round(rows.Sum(x => x.EnergyKwh), 2),
            TotalReactiveEnergyKvarh = Math.Round(rows.Sum(x => x.ReactiveEnergyKvarh), 2),
            AvgActivePowerW = Math.Round(WeightedAverage(rows, x => x.ActivePower), 1),
            AvgPowerFactor = Math.Round(WeightedAverage(rows, x => x.PowerFactor), 3),
            AvgVoltage = Math.Round(WeightedAverage(rows, x => x.Voltage), 1),
            AvgCurrent = Math.Round(WeightedAverage(rows, x => x.Current), 2),
            AvgFrequency = Math.Round(WeightedAverage(rows, x => x.Frequency), 2),
            PeakActivePowerW = Math.Round(rows.Max(x => x.MaxActivePower), 1),
            SensorCount = sensorCount,
            AlertCount = SafeIntSum(rows.Select(x => x.AlertSampleCount))
        };
    }

    private static CardStats BuildCardStats(
        IReadOnlyCollection<SensorEnergyBucketRow> rows,
        int sensorCount)
    {
        return new CardStats
        {
            EnergyKwh = Math.Round(rows.Sum(x => x.EnergyKwh), 2),
            AveragePowerFactor = rows.Count == 0
                ? 0
                : Math.Round(WeightedAverage(rows, x => x.PowerFactor), 3),
            SensorCount = sensorCount,
            AlertCount = SafeIntSum(rows.Select(x => x.AlertSampleCount))
        };
    }

    private static int SafeIntSum(IEnumerable<int> values)
    {
        long total = 0;
        foreach (var value in values)
        {
            total += value;
            if (total >= int.MaxValue)
                return int.MaxValue;
        }

        return (int)total;
    }

    private static List<TimeSeriesPointDto> BuildEnergySeries(
        IReadOnlyCollection<SensorEnergyBucketRow> rows,
        DateTime from,
        DateTime to)
    {
        var duration = to - from;
        var bucket = duration <= TimeSpan.FromDays(31)
            ? TimeSpan.FromHours(1)
            : duration <= TimeSpan.FromDays(120)
                ? TimeSpan.FromHours(6)
                : TimeSpan.FromDays(1);

        return rows
            .GroupBy(x => FloorTimestamp(x.At, bucket))
            .Select(g => new TimeSeriesPointDto
            {
                Timestamp = g.Key,
                Value = Math.Round(g.Sum(x => x.EnergyKwh), 3)
            })
            .OrderBy(x => x.Timestamp)
            .ToList();
    }

    private static DateTime FloorTimestamp(DateTime value, TimeSpan bucket)
    {
        value = EnsureUtc(value);
        if (bucket >= TimeSpan.FromDays(1))
            return value.Date;

        var ticks = value.Ticks - value.Ticks % bucket.Ticks;
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    private static Dictionary<Guid, List<SensorEnergyBucketRow>> GroupRowsBySensor(
        IEnumerable<SensorEnergyBucketRow> rows) =>
        rows.GroupBy(x => x.SensorId)
            .ToDictionary(x => x.Key, x => x.ToList());

    private static IReadOnlyCollection<SensorEnergyBucketRow> RowsForChild(
        Guid childId,
        IReadOnlyDictionary<Guid, HashSet<Guid>> sensorIdsByChild,
        IReadOnlyDictionary<Guid, List<SensorEnergyBucketRow>> rowsBySensor)
    {
        if (!sensorIdsByChild.TryGetValue(childId, out var sensorIds) || sensorIds.Count == 0)
            return Array.Empty<SensorEnergyBucketRow>();

        var rows = new List<SensorEnergyBucketRow>();
        foreach (var sensorId in sensorIds)
        {
            if (rowsBySensor.TryGetValue(sensorId, out var sensorRows))
                rows.AddRange(sensorRows);
        }

        return rows;
    }

    private static Dictionary<Guid, HashSet<Guid>> BuildChildSensorLookup(
        IEnumerable<ChildSensorMap> mappings) =>
        mappings
            .GroupBy(x => x.ChildId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.SensorId).ToHashSet());

    // ─── Sensor traversal ─────────────────────────────────────────────────────

    private IQueryable<Guid> SensorsByBusiness(Guid businessId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted && s.device.fk_business == businessId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByFacility(Guid facilityId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.floor.building.fk_facility == facilityId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByBuilding(Guid buildingId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.floor.fk_building == buildingId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByFloor(Guid floorId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.fk_floor == floorId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsBySection(Guid sectionId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.fk_section == sectionId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByOffice(Guid officeId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted && s.device.fk_office == officeId)
            .Select(s => s.sensor_id);

    private IQueryable<Guid> SensorsByDevice(Guid deviceId) =>
        _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && s.fk_device == deviceId)
            .Select(s => s.sensor_id);

    // ─── Business dashboard ───────────────────────────────────────────────────

    public async Task<BusinessDashboardDto> GetBusinessAsync(
        Guid businessId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByBusiness(businessId), from, to);

        var businessName = await _db.tbl_business
            .AsNoTracking()
            .Where(x => x.business_id == businessId && !x.is_deleted)
            .Select(x => x.business_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var facilities = await _db.tbl_facility
            .AsNoTracking()
            .Where(x => x.fk_business == businessId && !x.is_deleted)
            .Select(x => new { x.facility_id, x.facility_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.floor.building.facility.fk_business == businessId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.device.office.section.floor.building.fk_facility,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var sensorLookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = facilities.Select(f =>
        {
            var rows = RowsForChild(f.facility_id, sensorLookup, rowsBySensor);
            var sensorCount = sensorLookup.TryGetValue(f.facility_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, sensorCount);
            return new FacilityCardDto
            {
                FacilityId = f.facility_id,
                FacilityName = f.facility_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new BusinessDashboardDto
        {
            BusinessId = businessId,
            BusinessName = businessName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Facilities = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Facility dashboard ───────────────────────────────────────────────────

    public async Task<FacilityDashboardDto> GetFacilityAsync(
        Guid facilityId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByFacility(facilityId), from, to);

        var facilityName = await _db.tbl_facility
            .AsNoTracking()
            .Where(x => x.facility_id == facilityId && !x.is_deleted)
            .Select(x => x.facility_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var buildings = await _db.tbl_building
            .AsNoTracking()
            .Where(x => x.fk_facility == facilityId && !x.is_deleted)
            .Select(x => new { x.building_id, x.building_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.floor.building.fk_facility == facilityId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.device.office.section.floor.fk_building,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var lookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = buildings.Select(b =>
        {
            var rows = RowsForChild(b.building_id, lookup, rowsBySensor);
            var count = lookup.TryGetValue(b.building_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, count);
            return new BuildingCardDto
            {
                BuildingId = b.building_id,
                BuildingName = b.building_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new FacilityDashboardDto
        {
            FacilityId = facilityId,
            FacilityName = facilityName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Buildings = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Building dashboard ───────────────────────────────────────────────────

    public async Task<BuildingDashboardDto> GetBuildingAsync(
        Guid buildingId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByBuilding(buildingId), from, to);

        var buildingName = await _db.tbl_building
            .AsNoTracking()
            .Where(x => x.building_id == buildingId && !x.is_deleted)
            .Select(x => x.building_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var floors = await _db.tbl_floor
            .AsNoTracking()
            .Where(x => x.fk_building == buildingId && !x.is_deleted)
            .Select(x => new { x.floor_id, x.floor_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.floor.fk_building == buildingId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.device.office.section.fk_floor,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var lookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = floors.Select(f =>
        {
            var rows = RowsForChild(f.floor_id, lookup, rowsBySensor);
            var count = lookup.TryGetValue(f.floor_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, count);
            return new FloorCardDto
            {
                FloorId = f.floor_id,
                FloorName = f.floor_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new BuildingDashboardDto
        {
            BuildingId = buildingId,
            BuildingName = buildingName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Floors = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Floor dashboard ──────────────────────────────────────────────────────

    public async Task<FloorDashboardDto> GetFloorAsync(
        Guid floorId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByFloor(floorId), from, to);

        var floorName = await _db.tbl_floor
            .AsNoTracking()
            .Where(x => x.floor_id == floorId && !x.is_deleted)
            .Select(x => x.floor_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var sections = await _db.tbl_section
            .AsNoTracking()
            .Where(x => x.fk_floor == floorId && !x.is_deleted)
            .Select(x => new { x.section_id, x.section_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.section.fk_floor == floorId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.device.office.fk_section,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var lookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = sections.Select(s =>
        {
            var rows = RowsForChild(s.section_id, lookup, rowsBySensor);
            var count = lookup.TryGetValue(s.section_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, count);
            return new SectionCardDto
            {
                SectionId = s.section_id,
                SectionName = s.section_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new FloorDashboardDto
        {
            FloorId = floorId,
            FloorName = floorName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Sections = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Section dashboard ────────────────────────────────────────────────────

    public async Task<SectionDashboardDto> GetSectionAsync(
        Guid sectionId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsBySection(sectionId), from, to);

        var sectionName = await _db.tbl_section
            .AsNoTracking()
            .Where(x => x.section_id == sectionId && !x.is_deleted)
            .Select(x => x.section_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var offices = await _db.tbl_office
            .AsNoTracking()
            .Where(x => x.fk_section == sectionId && !x.is_deleted)
            .Select(x => new { x.office_id, x.office_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted &&
                        s.device.office.fk_section == sectionId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.device.fk_office,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var lookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = offices.Select(o =>
        {
            var rows = RowsForChild(o.office_id, lookup, rowsBySensor);
            var count = lookup.TryGetValue(o.office_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, count);
            return new OfficeCardDto
            {
                OfficeId = o.office_id,
                OfficeName = o.office_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new SectionDashboardDto
        {
            SectionId = sectionId,
            SectionName = sectionName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Offices = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Office dashboard ─────────────────────────────────────────────────────

    public async Task<OfficeDashboardDto> GetOfficeAsync(
        Guid officeId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByOffice(officeId), from, to);

        var officeName = await _db.tbl_office
            .AsNoTracking()
            .Where(x => x.office_id == officeId && !x.is_deleted)
            .Select(x => x.office_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var devices = await _db.tbl_device
            .AsNoTracking()
            .Where(x => x.fk_office == officeId && !x.is_deleted)
            .Select(x => new { x.device_id, x.device_name })
            .ToListAsync();

        var mappings = await _db.tbl_sensor
            .AsNoTracking()
            .Where(s => !s.is_deleted && !s.device.is_deleted && s.device.fk_office == officeId)
            .Select(s => new ChildSensorMap
            {
                ChildId = s.fk_device,
                SensorId = s.sensor_id
            })
            .ToListAsync();

        var lookup = BuildChildSensorLookup(mappings);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);
        var cards = devices.Select(d =>
        {
            var rows = RowsForChild(d.device_id, lookup, rowsBySensor);
            var count = lookup.TryGetValue(d.device_id, out var ids) ? ids.Count : 0;
            var stats = BuildCardStats(rows, count);
            return new DeviceCardDto
            {
                DeviceId = d.device_id,
                DeviceName = d.device_name,
                TotalActiveEnergyKwh = stats.EnergyKwh,
                AvgPowerFactor = stats.AveragePowerFactor,
                SensorCount = stats.SensorCount,
                AlertCount = stats.AlertCount
            };
        }).ToList();

        return new OfficeDashboardDto
        {
            OfficeId = officeId,
            OfficeName = officeName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Devices = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    // ─── Device dashboard ─────────────────────────────────────────────────────

    public async Task<DeviceDashboardDto> GetDeviceAsync(
        Guid deviceId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var scope = await LoadScopeAsync(SensorsByDevice(deviceId), from, to);

        var deviceName = await _db.tbl_device
            .AsNoTracking()
            .Where(x => x.device_id == deviceId && !x.is_deleted)
            .Select(x => x.device_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var sensors = await _db.tbl_sensor
            .AsNoTracking()
            .Where(x => x.fk_device == deviceId && !x.is_deleted)
            .Select(x => new { x.sensor_id, x.sensor_name })
            .ToListAsync();

        var latest = await LoadLatestSensorStatesAsync(scope.SensorIds);
        var rowsBySensor = GroupRowsBySensor(scope.Rows);

        var cards = sensors.Select(sensor =>
        {
            rowsBySensor.TryGetValue(sensor.sensor_id, out var rows);
            latest.TryGetValue(sensor.sensor_id, out var state);
            return new SensorCardDto
            {
                SensorId = sensor.sensor_id,
                SensorName = sensor.sensor_name,
                LatestVoltage = state is null ? 0 : Math.Round(state.Voltage, 1),
                LatestCurrent = state is null ? 0 : Math.Round(state.Current, 2),
                LatestActivePower = state is null ? 0 : Math.Round(state.ActivePower, 1),
                LatestPowerFactor = state is null ? 0 : Math.Round(state.PowerFactor, 3),
                TotalActiveEnergyKwh = Math.Round(rows?.Sum(x => x.EnergyKwh) ?? 0, 2),
                HasAlert = state is not null && IsAlert(state.PowerFactor, state.Voltage)
            };
        }).ToList();

        return new DeviceDashboardDto
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Kpis = BuildKpis(scope.Rows, scope.SensorIds.Count),
            Sensors = cards,
            HourlyEnergy = BuildEnergySeries(scope.Rows, from, to)
        };
    }

    private async Task<Dictionary<Guid, LatestSensorState>> LoadLatestSensorStatesAsync(
        IReadOnlyCollection<Guid> sensorIds)
    {
        var latest = await _energyStore.LoadLatestAsync(sensorIds);
        return latest.ToDictionary(
            x => x.Key,
            x => new LatestSensorState
            {
                SensorId = x.Value.SensorId,
                At = x.Value.At,
                PacketId = x.Value.PacketId,
                Voltage = x.Value.Voltage,
                Current = x.Value.Current,
                ActivePower = x.Value.ActivePower,
                PowerFactor = x.Value.PowerFactor
            });
    }

    private static bool IsAlert(double powerFactor, double voltage) =>
        powerFactor < 0.85 || voltage is < 210 or > 250;

    // ─── Sensor dashboard full detail ─────────────────────────────────────────

    public async Task<SensorDashboardDto> GetSensorAsync(
        Guid sensorId,
        DashboardQueryParams q)
    {
        var (from, to) = ResolveRange(q);
        var sensorName = await _db.tbl_sensor
            .AsNoTracking()
            .Where(x => x.sensor_id == sensorId && !x.is_deleted)
            .Select(x => x.sensor_name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var rows = await _energyStore.LoadAsync(new[] { sensorId }, from, to);
        var chartRows = RollUpSensorRows(rows, to - from);

        var baseline = await _db.tbl_singal_phase_data
            .AsNoTracking()
            .Where(x => !x.is_deleted && x.fk_sensor == sensorId && x.created_at < from)
            .OrderByDescending(x => x.created_at)
            .ThenByDescending(x => x.packet_id)
            .Select(x => new
            {
                ActiveEnergy = (double)x.active_energy,
                ReactiveEnergy = (double)x.reactive_energy
            })
            .FirstOrDefaultAsync();

        var activeCumulative = baseline?.ActiveEnergy ?? 0;
        var reactiveCumulative = baseline?.ReactiveEnergy ?? 0;
        var activeEnergySeries = new List<TimeSeriesPointDto>(chartRows.Count);
        var reactiveEnergySeries = new List<TimeSeriesPointDto>(chartRows.Count);

        foreach (var row in chartRows)
        {
            activeCumulative += row.EnergyKwh;
            reactiveCumulative += row.ReactiveEnergyKvarh;
            activeEnergySeries.Add(new TimeSeriesPointDto
            {
                Timestamp = row.At,
                Value = Math.Round(activeCumulative, 3)
            });
            reactiveEnergySeries.Add(new TimeSeriesPointDto
            {
                Timestamp = row.At,
                Value = Math.Round(reactiveCumulative, 3)
            });
        }

        List<TimeSeriesPointDto> Series(Func<SensorEnergyBucketRow, double> selector) =>
            chartRows.Select(x => new TimeSeriesPointDto
            {
                Timestamp = x.At,
                Value = Math.Round(selector(x), 3)
            }).ToList();

        var totalPfSamples = rows.Sum(x =>
            x.PfExcellentCount + x.PfGoodCount + x.PfAcceptableCount + x.PfPoorCount);
        var pfDistribution = totalPfSamples == 0
            ? new PfDistributionDto()
            : new PfDistributionDto
            {
                ExcellentPct = Math.Round(rows.Sum(x => x.PfExcellentCount) * 100.0 / totalPfSamples, 1),
                GoodPct = Math.Round(rows.Sum(x => x.PfGoodCount) * 100.0 / totalPfSamples, 1),
                AcceptablePct = Math.Round(rows.Sum(x => x.PfAcceptableCount) * 100.0 / totalPfSamples, 1),
                PoorPct = Math.Round(rows.Sum(x => x.PfPoorCount) * 100.0 / totalPfSamples, 1)
            };

        var hourlyDemand = rows
            .GroupBy(x => x.At.Hour)
            .Select(g => new HourlyDemandDto
            {
                Hour = g.Key,
                AvgActivePowerW = Math.Round(WeightedAverage(g, x => x.ActivePower), 1)
            })
            .OrderBy(x => x.Hour)
            .ToList();

        var recentRaw = await _db.tbl_singal_phase_data
            .AsNoTracking()
            .Where(x => !x.is_deleted && x.fk_sensor == sensorId &&
                        x.created_at >= from && x.created_at < to)
            .OrderByDescending(x => x.created_at)
            .ThenByDescending(x => x.packet_id)
            .Take(50)
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
                x.reactive_energy
            })
            .ToListAsync();

        var recentReadings = recentRaw.Select(x => new RawReadingDto
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
        }).ToList();

        var anomalyRows = await _db.tbl_singal_phase_data
            .AsNoTracking()
            .Where(x => !x.is_deleted && x.fk_sensor == sensorId &&
                        x.created_at >= from && x.created_at < to &&
                        (x.volt < 210 || x.volt > 250 || x.power_factor < 0.85f))
            .OrderByDescending(x => x.created_at)
            .Take(20)
            .Select(x => new
            {
                At = x.created_at,
                Voltage = (double)x.volt,
                PowerFactor = (double)x.power_factor
            })
            .ToListAsync();

        var alerts = new List<AlertDto>();
        foreach (var row in anomalyRows)
        {
            if (row.Voltage < 210)
            {
                alerts.Add(new AlertDto
                {
                    Type = "danger",
                    Message = $"Voltage sag {row.Voltage:F1} V (< 210 V)",
                    Timestamp = row.At
                });
            }
            else if (row.Voltage > 250)
            {
                alerts.Add(new AlertDto
                {
                    Type = "danger",
                    Message = $"Voltage surge {row.Voltage:F1} V (> 250 V)",
                    Timestamp = row.At
                });
            }

            if (row.PowerFactor < 0.85)
            {
                alerts.Add(new AlertDto
                {
                    Type = "warning",
                    Message = $"Low power factor {row.PowerFactor:F3} at {row.At:HH:mm}",
                    Timestamp = row.At
                });
            }
        }

        return new SensorDashboardDto
        {
            SensorId = sensorId,
            SensorName = sensorName,
            Kpis = BuildKpis(rows, 1),
            Voltage = Series(x => x.Voltage),
            Current = Series(x => x.Current),
            ActivePower = Series(x => x.ActivePower),
            ReactivePower = Series(x => x.ReactivePower),
            ApparentPower = Series(x => x.ApparentPower),
            PowerFactor = Series(x => x.PowerFactor),
            Frequency = Series(x => x.Frequency),
            ActiveEnergy = activeEnergySeries,
            ReactiveEnergy = reactiveEnergySeries,
            PfDistribution = pfDistribution,
            HourlyDemand = hourlyDemand,
            RecentReadings = recentReadings,
            Alerts = alerts
                .OrderByDescending(x => x.Timestamp)
                .Take(20)
                .ToList()
        };
    }

    private static List<SensorEnergyBucketRow> RollUpSensorRows(
        IReadOnlyCollection<SensorEnergyBucketRow> rows,
        TimeSpan duration)
    {
        var bucket = duration <= TimeSpan.FromDays(2)
            ? TimeSpan.FromMinutes(15)
            : duration <= TimeSpan.FromDays(31)
                ? TimeSpan.FromHours(1)
                : duration <= TimeSpan.FromDays(120)
                    ? TimeSpan.FromHours(6)
                    : TimeSpan.FromDays(1);

        return rows
            .GroupBy(x => FloorTimestamp(x.At, bucket))
            .Select(g =>
            {
                var materialized = g.ToList();
                return new SensorEnergyBucketRow
                {
                    SensorId = materialized[0].SensorId,
                    UtilityName = materialized[0].UtilityName,
                    At = g.Key,
                    EnergyKwh = materialized.Sum(x => x.EnergyKwh),
                    ReactiveEnergyKvarh = materialized.Sum(x => x.ReactiveEnergyKvarh),
                    ActivePower = WeightedAverage(materialized, x => x.ActivePower),
                    MaxActivePower = materialized.Max(x => x.MaxActivePower),
                    Voltage = WeightedAverage(materialized, x => x.Voltage),
                    Current = WeightedAverage(materialized, x => x.Current),
                    ReactivePower = WeightedAverage(materialized, x => x.ReactivePower),
                    ApparentPower = WeightedAverage(materialized, x => x.ApparentPower),
                    PowerFactor = WeightedAverage(materialized, x => x.PowerFactor),
                    Frequency = WeightedAverage(materialized, x => x.Frequency),
                    SampleCount = SafeIntSum(materialized.Select(x => x.SampleCount)),
                    PfExcellentCount = SafeIntSum(materialized.Select(x => x.PfExcellentCount)),
                    PfGoodCount = SafeIntSum(materialized.Select(x => x.PfGoodCount)),
                    PfAcceptableCount = SafeIntSum(materialized.Select(x => x.PfAcceptableCount)),
                    PfPoorCount = SafeIntSum(materialized.Select(x => x.PfPoorCount)),
                    AlertSampleCount = SafeIntSum(materialized.Select(x => x.AlertSampleCount)),
                    FirstReadingAt = materialized.Min(x => x.FirstReadingAt),
                    LastReadingAt = materialized.Max(x => x.LastReadingAt),
                    ResetCount = SafeIntSum(materialized.Select(x => x.ResetCount)),
                    IgnoredSpikeCount = SafeIntSum(materialized.Select(x => x.IgnoredSpikeCount))
                };
            })
            .OrderBy(x => x.At)
            .ToList();
    }

    private sealed class ScopeData
    {
        public List<Guid> SensorIds { get; init; } = new();
        public List<SensorEnergyBucketRow> Rows { get; init; } = new();
    }

    private sealed class ChildSensorMap
    {
        public Guid ChildId { get; init; }
        public Guid SensorId { get; init; }
    }

    private sealed class CardStats
    {
        public double EnergyKwh { get; init; }
        public double AveragePowerFactor { get; init; }
        public int SensorCount { get; init; }
        public int AlertCount { get; init; }
    }

    private sealed class LatestSensorState
    {
        public Guid SensorId { get; init; }
        public DateTime At { get; init; }
        public int PacketId { get; init; }
        public double Voltage { get; init; }
        public double Current { get; init; }
        public double ActivePower { get; init; }
        public double PowerFactor { get; init; }
    }
}
