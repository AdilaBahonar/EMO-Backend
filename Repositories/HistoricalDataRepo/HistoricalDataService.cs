using System.Globalization;
using System.Text;
using EMO.Models.DBModels;
using EMO.Models.DTOs.HistoricalDataDTOs;
using EMO.Repositories.DeepDiveRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.HistoricalDataRepo;

public interface IHistoricalDataService
{
    Task<HistoricalDataResponseDto?> GetAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> ExportCsvAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        CancellationToken cancellationToken = default);

    Task<HistoricalDataResponseDto?> GetTenantAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        IReadOnlyCollection<Guid> allowedOfficeIds,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> ExportTenantCsvAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        IReadOnlyCollection<Guid> allowedOfficeIds,
        CancellationToken cancellationToken = default);
}

public sealed class HistoricalDataService : IHistoricalDataService
{
    private static readonly TimeSpan MaximumRange = TimeSpan.FromDays(366 * 3);
    private readonly DBUserManagementContext _db;
    private readonly ISensorEnergyAggregateStore _aggregateStore;

    public HistoricalDataService(
        DBUserManagementContext db,
        ISensorEnergyAggregateStore aggregateStore)
    {
        _db = db;
        _aggregateStore = aggregateStore;
    }

    public Task<HistoricalDataResponseDto?> GetAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        CancellationToken cancellationToken = default) =>
        GetCoreAsync(level, id, query, null, cancellationToken);

    public Task<HistoricalDataResponseDto?> GetTenantAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        IReadOnlyCollection<Guid> allowedOfficeIds,
        CancellationToken cancellationToken = default) =>
        GetCoreAsync(level, id, query, allowedOfficeIds, cancellationToken);

    private async Task<HistoricalDataResponseDto?> GetCoreAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        IReadOnlyCollection<Guid>? allowedOfficeIds,
        CancellationToken cancellationToken)
    {
        level = NormalizeLevel(level);
        var interval = NormalizeInterval(query.Interval);
        var timeZone = ResolveTimeZone(query.TimeZone);
        var fromUtc = ConvertLocalToUtc(query.From, timeZone);
        var toUtc = ConvertLocalToUtc(query.To, timeZone);

        ValidateRange(fromUtc, toUtc, interval);

        var entityName = await ResolveEntityNameAsync(level, id, cancellationToken);
        if (entityName is null) return null;

        var sensorIds = await ResolveSensorIdsAsync(level, id, allowedOfficeIds, cancellationToken);
        var rows = await _aggregateStore.LoadAsync(sensorIds, fromUtc, toUtc, cancellationToken);
        var points = BuildPoints(rows, interval, timeZone, fromUtc, toUtc);

        return new HistoricalDataResponseDto
        {
            Level = level,
            EntityId = id,
            EntityName = entityName,
            Interval = interval,
            TimeZone = timeZone.Id,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            SensorCount = sensorIds.Count,
            PointCount = points.Count,
            Points = points
        };
    }

    public async Task<(byte[] Content, string FileName)> ExportCsvAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(level, id, query, cancellationToken)
            ?? throw new KeyNotFoundException("The requested dashboard scope was not found.");
        return BuildCsv(response);
    }

    public async Task<(byte[] Content, string FileName)> ExportTenantCsvAsync(
        string level,
        Guid id,
        HistoricalDataQueryDto query,
        IReadOnlyCollection<Guid> allowedOfficeIds,
        CancellationToken cancellationToken = default)
    {
        var response = await GetTenantAsync(level, id, query, allowedOfficeIds, cancellationToken)
            ?? throw new KeyNotFoundException("The requested dashboard scope was not found.");
        return BuildCsv(response);
    }

    private static (byte[] Content, string FileName) BuildCsv(HistoricalDataResponseDto response)
    {
        var csv = new StringBuilder();
        // UTF-8 BOM helps Excel display names and symbols correctly.
        csv.Append('\uFEFF');
        csv.AppendLine("Scope Level,Scope Name,Interval,Timezone,Bucket Start UTC,Bucket End UTC,Bucket Start Local,Bucket End Local,Energy kWh,Reactive Energy kvarh,Average Active Power W,Maximum Active Power W,Average Voltage V,Average Current A,Average Power Factor,Average Frequency Hz,Sample Count,Reset Count,Rejected Spike Count");

        foreach (var point in response.Points)
        {
            csv.Append(Csv(response.Level)).Append(',')
                .Append(Csv(response.EntityName)).Append(',')
                .Append(Csv(response.Interval)).Append(',')
                .Append(Csv(response.TimeZone)).Append(',')
                .Append(Csv(point.BucketStartUtc.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                .Append(Csv(point.BucketEndUtc.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                .Append(Csv(point.BucketStartLocal)).Append(',')
                .Append(Csv(point.BucketEndLocal)).Append(',')
                .Append(Number(point.EnergyKwh)).Append(',')
                .Append(Number(point.ReactiveEnergyKvarh)).Append(',')
                .Append(Number(point.AverageActivePowerW)).Append(',')
                .Append(Number(point.MaximumActivePowerW)).Append(',')
                .Append(Number(point.AverageVoltageV)).Append(',')
                .Append(Number(point.AverageCurrentA)).Append(',')
                .Append(Number(point.AveragePowerFactor)).Append(',')
                .Append(Number(point.AverageFrequencyHz)).Append(',')
                .Append(point.SampleCount).Append(',')
                .Append(point.ResetCount).Append(',')
                .Append(point.RejectedSpikeCount)
                .AppendLine();
        }

        var safeName = string.Concat(response.EntityName.Select(ch =>
            char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')).Trim('-');
        if (string.IsNullOrWhiteSpace(safeName)) safeName = response.Level;
        var fileName = $"{safeName}-{response.Interval}-{response.FromUtc:yyyyMMdd}-{response.ToUtc:yyyyMMdd}.csv";
        return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
    }

    private static List<HistoricalDataPointDto> BuildPoints(
        List<SensorEnergyBucketRow> rows,
        string interval,
        TimeZoneInfo timeZone,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var grouped = rows
            .Where(x => x.At >= fromUtc && x.At < toUtc)
            .GroupBy(x => ResolveBucketStartUtc(x.At, interval, timeZone))
            .OrderBy(x => x.Key);

        var result = new List<HistoricalDataPointDto>();
        foreach (var bucket in grouped)
        {
            var bucketStartUtc = bucket.Key;
            var bucketEndUtc = ResolveBucketEndUtc(bucketStartUtc, interval, timeZone);
            if (bucketEndUtc > toUtc) bucketEndUtc = toUtc;

            var sensorPower = bucket
                .GroupBy(x => x.SensorId)
                .Select(sensor => Weighted(sensor, x => x.ActivePower))
                .Sum();
            var sensorMaximumPower = bucket
                .GroupBy(x => x.SensorId)
                .Select(sensor => sensor.Max(x => x.MaxActivePower))
                .Sum();
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(bucketStartUtc, timeZone);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(bucketEndUtc, timeZone);

            result.Add(new HistoricalDataPointDto
            {
                BucketStartUtc = bucketStartUtc,
                BucketEndUtc = bucketEndUtc,
                BucketStartLocal = FormatLocal(localStart, timeZone),
                BucketEndLocal = FormatLocal(localEnd, timeZone),
                Label = interval switch
                {
                    "month" => localStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    "day" => localStart.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    _ => localStart.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)
                },
                EnergyKwh = Round(bucket.Sum(x => x.EnergyKwh), 6),
                ReactiveEnergyKvarh = Round(bucket.Sum(x => x.ReactiveEnergyKvarh), 6),
                AverageActivePowerW = Round(sensorPower, 3),
                MaximumActivePowerW = Round(sensorMaximumPower, 3),
                AverageVoltageV = Round(Weighted(bucket, x => x.Voltage), 3),
                AverageCurrentA = Round(Weighted(bucket, x => x.Current), 3),
                AveragePowerFactor = Round(Weighted(bucket, x => x.PowerFactor), 4),
                AverageFrequencyHz = Round(Weighted(bucket, x => x.Frequency), 3),
                SampleCount = bucket.Sum(x => Math.Max(0, x.SampleCount)),
                ResetCount = bucket.Sum(x => Math.Max(0, x.ResetCount)),
                RejectedSpikeCount = bucket.Sum(x => Math.Max(0, x.IgnoredSpikeCount))
            });
        }

        return result;
    }

    private async Task<List<Guid>> ResolveSensorIdsAsync(
        string level,
        Guid id,
        IReadOnlyCollection<Guid>? allowedOfficeIds,
        CancellationToken cancellationToken)
    {
        var officeScope = allowedOfficeIds?.Distinct().ToArray();
        IQueryable<Guid> query = level switch
        {
            "business" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted && x.device.fk_business == id)
                .Select(x => x.sensor_id),
            "facility" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted &&
                            x.device.office.section.floor.building.fk_facility == id)
                .Select(x => x.sensor_id),
            "building" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted &&
                            x.device.office.section.floor.fk_building == id)
                .Select(x => x.sensor_id),
            "floor" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted &&
                            x.device.office.section.fk_floor == id)
                .Select(x => x.sensor_id),
            "section" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted &&
                            x.device.office.fk_section == id)
                .Select(x => x.sensor_id),
            "office" => _db.tbl_sensor
                .Where(x => !x.is_deleted && !x.device.is_deleted && x.device.fk_office == id)
                .Select(x => x.sensor_id),
            "device" => _db.tbl_sensor
                .Where(x => !x.is_deleted && x.fk_device == id)
                .Select(x => x.sensor_id),
            "sensor" => _db.tbl_sensor
                .Where(x => !x.is_deleted && x.sensor_id == id)
                .Select(x => x.sensor_id),
            _ => throw new ArgumentException("Unknown level.")
        };

        if (officeScope is { Length: > 0 })
        {
            query = from sensorId in query
                    join sensor in _db.tbl_sensor.AsNoTracking() on sensorId equals sensor.sensor_id
                    where officeScope.Contains(sensor.device.fk_office)
                    select sensorId;
        }
        else if (allowedOfficeIds is not null)
        {
            return new List<Guid>();
        }

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    private async Task<string?> ResolveEntityNameAsync(
        string level,
        Guid id,
        CancellationToken cancellationToken) => level switch
    {
        "business" => await _db.tbl_business.Where(x => x.business_id == id && !x.is_deleted)
            .Select(x => x.business_name).FirstOrDefaultAsync(cancellationToken),
        "facility" => await _db.tbl_facility.Where(x => x.facility_id == id && !x.is_deleted)
            .Select(x => x.facility_name).FirstOrDefaultAsync(cancellationToken),
        "building" => await _db.tbl_building.Where(x => x.building_id == id && !x.is_deleted)
            .Select(x => x.building_name).FirstOrDefaultAsync(cancellationToken),
        "floor" => await _db.tbl_floor.Where(x => x.floor_id == id && !x.is_deleted)
            .Select(x => x.floor_name).FirstOrDefaultAsync(cancellationToken),
        "section" => await _db.tbl_section.Where(x => x.section_id == id && !x.is_deleted)
            .Select(x => x.section_name).FirstOrDefaultAsync(cancellationToken),
        "office" => await _db.tbl_office.Where(x => x.office_id == id && !x.is_deleted)
            .Select(x => x.office_name).FirstOrDefaultAsync(cancellationToken),
        "device" => await _db.tbl_device.Where(x => x.device_id == id && !x.is_deleted)
            .Select(x => x.device_name).FirstOrDefaultAsync(cancellationToken),
        "sensor" => await _db.tbl_sensor.Where(x => x.sensor_id == id && !x.is_deleted)
            .Select(x => x.sensor_name).FirstOrDefaultAsync(cancellationToken),
        _ => null
    };

    private static string NormalizeLevel(string value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "business" => "business",
            "facility" => "facility",
            "building" => "building",
            "floor" => "floor",
            "section" => "section",
            "office" => "office",
            "device" => "device",
            "sensor" => "sensor",
            _ => throw new ArgumentException("Unknown level. Use business, facility, building, floor, section, office, device, or sensor.")
        };

    private static string NormalizeInterval(string value) =>
        (value ?? "day").Trim().ToLowerInvariant() switch
        {
            "15m" or "15minute" or "15minutes" => "15minute",
            "hour" or "hourly" or "1h" => "hour",
            "day" or "daily" or "1d" => "day",
            "month" or "monthly" or "1mo" => "month",
            _ => throw new ArgumentException("Interval must be 15minute, hour, day, or month.")
        };

    private static void ValidateRange(DateTime fromUtc, DateTime toUtc, string interval)
    {
        if (toUtc <= fromUtc)
            throw new ArgumentException("The end of the range must be after the start.");
        var span = toUtc - fromUtc;
        if (span > MaximumRange)
            throw new ArgumentException("The maximum historical range is three years.");
        if (interval == "15minute" && span > TimeSpan.FromDays(31))
            throw new ArgumentException("15-minute exports are limited to 31 days. Choose hourly, daily, or monthly.");
        if (interval == "hour" && span > TimeSpan.FromDays(366))
            throw new ArgumentException("Hourly exports are limited to one year. Choose daily or monthly.");
    }

    private static TimeZoneInfo ResolveTimeZone(string value)
    {
        var id = string.IsNullOrWhiteSpace(value) ? "UTC" : value.Trim();
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"The reporting timezone '{id}' is not supported by the server.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException($"The reporting timezone '{id}' is invalid.");
        }
    }

    private static DateTime ConvertLocalToUtc(DateTime value, TimeZoneInfo zone)
    {
        var local = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local))
            local = local.AddHours(1);
        if (zone.IsAmbiguousTime(local))
        {
            var offset = zone.GetAmbiguousTimeOffsets(local).Max();
            return new DateTimeOffset(local, offset).UtcDateTime;
        }
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    private static DateTime ResolveBucketStartUtc(
        DateTime atUtc,
        string interval,
        TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(EnsureUtc(atUtc), zone);
        var start = interval switch
        {
            "month" => new DateTime(local.Year, local.Month, 1, 0, 0, 0),
            "day" => new DateTime(local.Year, local.Month, local.Day, 0, 0, 0),
            "hour" => new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0),
            _ => new DateTime(local.Year, local.Month, local.Day, local.Hour,
                local.Minute - local.Minute % 15, 0)
        };
        return ConvertLocalToUtc(start, zone);
    }

    private static DateTime ResolveBucketEndUtc(
        DateTime bucketStartUtc,
        string interval,
        TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(bucketStartUtc, zone);
        var end = interval switch
        {
            "month" => new DateTime(local.Year, local.Month, 1).AddMonths(1),
            "day" => new DateTime(local.Year, local.Month, local.Day).AddDays(1),
            "hour" => new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0).AddHours(1),
            _ => new DateTime(local.Year, local.Month, local.Day, local.Hour,
                local.Minute - local.Minute % 15, 0).AddMinutes(15)
        };
        return ConvertLocalToUtc(end, zone);
    }

    private static double Weighted(
        IEnumerable<SensorEnergyBucketRow> rows,
        Func<SensorEnergyBucketRow, double> selector)
    {
        double total = 0;
        long samples = 0;
        foreach (var row in rows)
        {
            var count = Math.Max(0, row.SampleCount);
            if (count == 0) continue;
            total += selector(row) * count;
            samples += count;
        }
        return samples == 0 ? 0 : total / samples;
    }

    private static string FormatLocal(DateTime value, TimeZoneInfo zone) =>
        $"{value:yyyy-MM-dd HH:mm:ss} [{zone.Id}]";

    private static string Csv(string value) =>
        $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    private static string Number(double value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);

    private static double Round(double value, int digits) => Math.Round(value, digits);

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
