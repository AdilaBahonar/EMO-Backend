using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.DeepDiveDTOs;
using EMO.Models.DTOs.EnergyDashboardDTOs;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.DeepDiveRepo;

public interface IDeepDiveService
{
    Task<DeepDiveResponseDto?> GetAsync(string level, Guid id, DeepDiveQueryDto query);
    Task<DeepDiveResponseDto?> GetTenantAsync(string level, Guid id, DeepDiveQueryDto query, IReadOnlyCollection<Guid> allowedOfficeIds);
    Task WarmScopeAsync(string level, Guid id, string range, CancellationToken cancellationToken = default);
}

public class DeepDiveService : IDeepDiveService
{
    private const string EnergyConfigurationRoute = "/settings/energy";
    private readonly DBUserManagementContext _db;
    private readonly ISensorEnergyAggregateStore _energyAggregateStore;

    public DeepDiveService(
        DBUserManagementContext db,
        ISensorEnergyAggregateStore energyAggregateStore)
    {
        _db = db;
        _energyAggregateStore = energyAggregateStore;
    }

    public Task<DeepDiveResponseDto?> GetAsync(string level, Guid id, DeepDiveQueryDto query) =>
        GetInternalAsync(level, id, query, null);

    public Task<DeepDiveResponseDto?> GetTenantAsync(
        string level,
        Guid id,
        DeepDiveQueryDto query,
        IReadOnlyCollection<Guid> allowedOfficeIds) =>
        GetInternalAsync(level, id, query, allowedOfficeIds.ToHashSet());

    private async Task<DeepDiveResponseDto?> GetInternalAsync(
        string level,
        Guid id,
        DeepDiveQueryDto query,
        HashSet<Guid>? allowedOfficeIds)
    {
        level = NormalizeLevel(level);
        var rangeKey = NormalizeRangeKey(query);
        var (from, aggregateTo) = ResolveRange(query);
        var responseTo = ResolveResponseTo(query, aggregateTo);
        if (aggregateTo <= from || responseTo <= from) return null;

        var isTenantScoped = allowedOfficeIds is not null;
        DeepDiveResponseDto? result = null;
        var servedFromAggregate = false;

        // Business aggregates must never be shared with a tenant-scoped request,
        // because the same hierarchy id can represent a different set of offices.
        if (!isTenantScoped && !query.ForceRefresh)
        {
            result = await TryReadAggregateAsync(level, id, rangeKey, from, aggregateTo);
            servedFromAggregate = result is not null;
        }

        if (result is null)
        {
            result = await BuildAsync(level, id, query, allowedOfficeIds);
            if (result is null) return null;

            result.CalculatedAt = DateTime.UtcNow;
            result.ServedFromAggregate = false;

            if (!isTenantScoped)
            {
                var businessId = await ResolveBusinessIdAsync(level, id) ?? Guid.Empty;
                await UpsertAggregateAsync(
                    level, id, businessId, rangeKey, from, aggregateTo, "deepdive", result);
            }
        }

        await RefreshLiveOverlayAsync(level, id, result, allowedOfficeIds);

        if (!IsCustomRange(query) && responseTo > aggregateTo)
        {
            await AppendLatestRangeAsync(
                level, id, rangeKey, aggregateTo, responseTo, result, query.TimeZone, allowedOfficeIds);
        }

        result.CalculatedAt = DateTime.UtcNow;
        result.ServedFromAggregate = servedFromAggregate;
        return result;
    }

    public async Task WarmScopeAsync(
        string level,
        Guid id,
        string range,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await GetAsync(level, id, new DeepDiveQueryDto
        {
            Range = range,
            ForceRefresh = true
        });
    }

    private async Task<DeepDiveResponseDto?> BuildAsync(string level, Guid id, DeepDiveQueryDto query, HashSet<Guid>? allowedOfficeIds = null)
    {
        level = level.ToLowerInvariant();
        var (from, to) = ResolveRange(query);
        if (to <= from) return null;

        var entity = await ResolveEntityAsync(level, id);
        if (entity is null) return null;

        var duration = to - from;
        var previousFrom = from - duration;
        var previousTo = from;
        var sensorIds = await ResolveSensorIdsAsync(level, id, allowedOfficeIds);
        var businessId = await ResolveBusinessIdAsync(level, id);

        var currentRows = await LoadRowsAsync(sensorIds, from, to);
        var previousRows = await LoadRowsAsync(sensorIds, previousFrom, previousTo);
        var config = businessId.HasValue
            ? await LoadConfigurationAsync(businessId.Value, sensorIds, level, id, query.TimeZone)
            : new ConfigurationContext();

        var energy = CalculateEnergy(currentRows);
        var previousEnergy = CalculateEnergy(previousRows);
        var peakOffPeak = CalculatePeakOffPeak(currentRows, config);
        var trend = BuildTrend(currentRows, previousRows, from, to, config);
        var demand = BuildDemand(currentRows, config);
        var health = await BuildSensorHealthAsync(
            sensorIds, config.OnlineThresholdSeconds);
        var issues = health.Issues;
        var children = await BuildChildrenAsync(
            level, id, currentRows, previousRows, energy, config, health, allowedOfficeIds);
        var suggestions = businessId.HasValue
            ? await LoadSuggestionsAsync(
                businessId.Value, sensorIds, from, to, config,
                allowedOfficeIds, includeBusinessLevel: allowedOfficeIds is null && level == "business")
            : new List<DeepDiveSuggestionDto>();

        var savingKwh = suggestions.Sum(x => x.EstimatedSavingKwh ?? 0);
        double? savingCost = config.Features.SavingsCostAnalysis
            ? suggestions.Where(x => x.EstimatedSavingCost.HasValue)
                .Sum(x => x.EstimatedSavingCost ?? 0)
            : null;

        double? estimatedCost = config.Features.CostAnalysis
            ? Round(CalculateCost(currentRows, config))
            : null;
        double? previousEstimatedCost = config.Features.CostAnalysis
            ? Round(CalculateCost(previousRows, config))
            : null;

        var sensorsWithReadings = currentRows
            .Where(x => x.SampleCount > 0)
            .Select(x => x.SensorId)
            .Distinct()
            .Count();
        var dataStatus = new DeepDiveDataStatusDto
        {
            HasReadings = sensorsWithReadings > 0,
            SensorCount = sensorIds.Count,
            SensorsWithReadings = sensorsWithReadings,
            FirstReadingAt = currentRows.Count > 0
                ? currentRows.Min(x => x.FirstReadingAt)
                : null,
            LastReadingAt = currentRows.Count > 0
                ? currentRows.Max(x => x.LastReadingAt)
                : null,
            Message = BuildDataMessage(
                sensorIds.Count,
                sensorsWithReadings,
                currentRows.Sum(x => x.SampleCount),
                currentRows.Sum(x => x.ResetCount),
                currentRows.Sum(x => x.IgnoredSpikeCount))
        };

        var response = new DeepDiveResponseDto
        {
            Level = level,
            EntityId = id,
            EntityName = entity.Value.name,
            ChildLevel = ChildLevel(level),
            From = from,
            To = to,
            Currency = config.Currency,
            Timezone = config.TimeZoneId,
            DataStatus = dataStatus,
            Configuration = config.Status,
            Features = config.Features,
            Breadcrumbs = await BuildBreadcrumbsAsync(level, id),
            Summary = new DeepDiveSummaryDto
            {
                EnergyKwh = Round(energy),
                PreviousEnergyKwh = Round(previousEnergy),
                EnergyChangePercent = ChangePercent(energy, previousEnergy),
                EstimatedCost = estimatedCost,
                PreviousEstimatedCost = previousEstimatedCost,
                CostChangePercent = estimatedCost.HasValue && previousEstimatedCost.HasValue
                    ? ChangePercent(estimatedCost.Value, previousEstimatedCost.Value)
                    : null,
                PeakDemandKw = demand.PeakDemandKw,
                PeakDemandAt = demand.PeakDemandAt,
                SavingOpportunityKwh = Round(savingKwh),
                SavingOpportunityCost = savingCost.HasValue ? Round(savingCost.Value) : null
            },
            Children = children,
            Trend = trend,
            PeakOffPeak = peakOffPeak,
            Demand = demand,
            ActiveIssues = issues,
            Suggestions = suggestions,
            Insight = BuildInsight(children, energy, config.Status)
        };

        response.CrmCharts = await BuildCrmChartsAsync(
            level, id, businessId ?? Guid.Empty, sensorIds, currentRows, from, to, config, query);
        return response;
    }

    private static (DateTime from, DateTime to) ResolveRange(DeepDiveQueryDto q)
    {
        var custom = IsCustomRange(q);
        var rawTo = EnsureUtc(q.To ?? DateTime.UtcNow);
        var key = NormalizeRangeKey(q);

        // Named ranges use a stable completed-period anchor so the worker and API
        // resolve the same cache key. The current live edge is merged separately.
        var to = custom
            ? rawTo
            : key switch
            {
                "1y" => rawTo.Date,
                "90d" => new DateTime(
                    rawTo.Year,
                    rawTo.Month,
                    rawTo.Day,
                    rawTo.Hour - rawTo.Hour % 6,
                    0,
                    0,
                    DateTimeKind.Utc),
                _ => new DateTime(
                    rawTo.Year,
                    rawTo.Month,
                    rawTo.Day,
                    rawTo.Hour,
                    0,
                    0,
                    DateTimeKind.Utc)
            };

        var from = q.From.HasValue
            ? EnsureUtc(q.From.Value)
            : key switch
            {
                "7d" => to.AddDays(-7),
                "30d" => to.AddDays(-30),
                "90d" => to.AddDays(-90),
                "1y" => to.AddYears(-1),
                _ => to.AddHours(-24)
            };

        return (from, to);
    }

    private static DateTime ResolveResponseTo(DeepDiveQueryDto query, DateTime aggregateTo)
    {
        if (query.To.HasValue)
            return EnsureUtc(query.To.Value);

        if (query.From.HasValue)
        {
            var now = DateTime.UtcNow;
            return new DateTime(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
        }

        var utcNow = DateTime.UtcNow;
        var currentMinute = new DateTime(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            utcNow.Hour,
            utcNow.Minute,
            0,
            DateTimeKind.Utc);
        return currentMinute < aggregateTo ? aggregateTo : currentMinute;
    }

    private static bool IsCustomRange(DeepDiveQueryDto query) =>
        query.From.HasValue || query.To.HasValue;

    private static string NormalizeLevel(string level) =>
        (level ?? string.Empty).Trim().ToLowerInvariant();

    private static string NormalizeRangeKey(DeepDiveQueryDto query)
    {
        if (IsCustomRange(query)) return "custom";
        return (query.Range ?? "24h").Trim().ToLowerInvariant() switch
        {
            "7d" => "7d",
            "30d" => "30d",
            "90d" => "90d",
            "1y" or "12m" or "365d" => "1y",
            _ => "24h"
        };
    }

    private static TimeSpan AggregateFreshness(string rangeKey) => rangeKey switch
    {
        "custom" => TimeSpan.FromMinutes(2),
        "1y" => TimeSpan.FromHours(26),
        "90d" => TimeSpan.FromHours(8),
        _ => TimeSpan.FromHours(2)
    };

    private async Task<DeepDiveResponseDto?> TryReadAggregateAsync(
        string level, Guid id, string rangeKey, DateTime from, DateTime to)
    {
        var freshAfter = DateTime.UtcNow - AggregateFreshness(rangeKey);
        var stored = await _db.tbl_dashboard_chart_aggregate
            .AsNoTracking()
            .Where(x => x.scope_level == level && x.scope_id == id &&
                        x.chart_type == "deepdive" && x.range_key == rangeKey &&
                        x.updated_at >= freshAfter && x.from_time == from && x.to_time == to)
            .OrderByDescending(x => x.updated_at)
            .FirstOrDefaultAsync();

        if (stored is null || string.IsNullOrWhiteSpace(stored.payload_json)) return null;
        try
        {
            return JsonSerializer.Deserialize<DeepDiveResponseDto>(stored.payload_json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<T?> TryReadChartAggregateAsync<T>(
        string level, Guid id, string chartType, string rangeKey, DateTime from, DateTime to)
    {
        var freshAfter = DateTime.UtcNow - AggregateFreshness(rangeKey);
        var stored = await _db.tbl_dashboard_chart_aggregate
            .AsNoTracking()
            .Where(x => x.scope_level == level && x.scope_id == id &&
                        x.chart_type == chartType && x.range_key == rangeKey &&
                        x.updated_at >= freshAfter && x.from_time == from && x.to_time == to)
            .OrderByDescending(x => x.updated_at)
            .FirstOrDefaultAsync();
        if (stored is null || string.IsNullOrWhiteSpace(stored.payload_json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(stored.payload_json);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private async Task UpsertAggregateAsync<T>(
        string level,
        Guid id,
        Guid businessId,
        string rangeKey,
        DateTime from,
        DateTime to,
        string chartType,
        T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var now = DateTime.UtcNow;

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tbl_dashboard_chart_aggregate
            (
                dashboard_chart_aggregate_id,
                fk_business,
                scope_level,
                scope_id,
                chart_type,
                range_key,
                from_time,
                to_time,
                payload_json,
                created_at,
                updated_at
            )
            VALUES
            (
                {Guid.NewGuid()},
                {businessId},
                {level},
                {id},
                {chartType},
                {rangeKey},
                {from},
                {to},
                {json},
                {now},
                {now}
            )
            ON CONFLICT (scope_level, scope_id, chart_type, range_key)
            DO UPDATE SET
                fk_business = EXCLUDED.fk_business,
                from_time = EXCLUDED.from_time,
                to_time = EXCLUDED.to_time,
                payload_json = EXCLUDED.payload_json,
                updated_at = EXCLUDED.updated_at;
            """);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private async Task<(Guid id, string name)?> ResolveEntityAsync(string level, Guid id)
    {
        string? name = level switch
        {
            "business" => await _db.tbl_business.Where(x => x.business_id == id && !x.is_deleted)
                .Select(x => x.business_name).FirstOrDefaultAsync(),
            "facility" => await _db.tbl_facility.Where(x => x.facility_id == id && !x.is_deleted)
                .Select(x => x.facility_name).FirstOrDefaultAsync(),
            "building" => await _db.tbl_building.Where(x => x.building_id == id && !x.is_deleted)
                .Select(x => x.building_name).FirstOrDefaultAsync(),
            "floor" => await _db.tbl_floor.Where(x => x.floor_id == id && !x.is_deleted)
                .Select(x => x.floor_name).FirstOrDefaultAsync(),
            "section" => await _db.tbl_section.Where(x => x.section_id == id && !x.is_deleted)
                .Select(x => x.section_name).FirstOrDefaultAsync(),
            "office" => await _db.tbl_office.Where(x => x.office_id == id && !x.is_deleted)
                .Select(x => x.office_name).FirstOrDefaultAsync(),
            "device" => await _db.tbl_device.Where(x => x.device_id == id && !x.is_deleted)
                .Select(x => x.device_name).FirstOrDefaultAsync(),
            "sensor" => await _db.tbl_sensor.Where(x => x.sensor_id == id && !x.is_deleted)
                .Select(x => x.sensor_name).FirstOrDefaultAsync(),
            _ => null
        };
        return string.IsNullOrWhiteSpace(name) ? null : (id, name);
    }

    private async Task<List<Guid>> ResolveSensorIdsAsync(
        string level,
        Guid id,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        bool OfficeAllowed(Guid officeId) => allowedOfficeIds is null || allowedOfficeIds.Contains(officeId);

        IQueryable<Guid> query = level switch
        {
            "business" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.fk_business == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "facility" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.floor.building.fk_facility == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "building" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.floor.fk_building == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "floor" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.fk_floor == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "section" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.fk_section == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "office" => _db.tbl_sensor.Where(s => !s.is_deleted && s.device.fk_office == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "device" => _db.tbl_sensor.Where(s => !s.is_deleted && s.fk_device == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            "sensor" => _db.tbl_sensor.Where(s => !s.is_deleted && s.sensor_id == id
                && (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office))).Select(s => s.sensor_id),
            _ => _db.tbl_sensor.Where(s => false).Select(s => s.sensor_id)
        };
        return await query.Distinct().ToListAsync();
    }

    private async Task<Guid?> ResolveBusinessIdAsync(string level, Guid id)
    {
        return level switch
        {
            "business" => id,
            "facility" => await _db.tbl_facility.Where(x => x.facility_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "building" => await _db.tbl_building.Where(x => x.building_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "floor" => await _db.tbl_floor.Where(x => x.floor_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "section" => await _db.tbl_section.Where(x => x.section_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "office" => await _db.tbl_office.Where(x => x.office_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "device" => await _db.tbl_device.Where(x => x.device_id == id).Select(x => (Guid?)x.fk_business).FirstOrDefaultAsync(),
            "sensor" => await _db.tbl_sensor.Where(x => x.sensor_id == id).Select(x => (Guid?)x.device.fk_business).FirstOrDefaultAsync(),
            _ => null
        };
    }

    private async Task RefreshLiveOverlayAsync(
        string level,
        Guid id,
        DeepDiveResponseDto cached,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        var sensorIds = await ResolveSensorIdsAsync(level, id, allowedOfficeIds);
        if (sensorIds.Count == 0)
        {
            cached.ActiveIssues = new List<DeepDiveIssueDto>();
            foreach (var child in cached.Children)
            {
                child.OnlineSensorCount = 0;
                child.IssueCount = 0;
            }
            return;
        }

        var businessId = await ResolveBusinessIdAsync(level, id);
        var onlineThresholdSeconds = businessId.HasValue
            ? await _db.tbl_business_dashboard_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId.Value && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .Select(x => x.online_sensor_threshold_seconds)
                .FirstOrDefaultAsync()
            : 120;
        if (onlineThresholdSeconds <= 0)
            onlineThresholdSeconds = 120;

        var health = await BuildSensorHealthAsync(sensorIds, onlineThresholdSeconds);
        cached.ActiveIssues = health.Issues;

        // Suggestions are a live overlay just like sensor health. Historical
        // chart snapshots may stay cached, but newly generated optimization
        // recommendations must appear without forcing a full Deep Dive rebuild.
        if (businessId.HasValue && cached.Features.OptimizationSuggestions)
        {
            cached.Suggestions = await LoadSuggestionsAsync(
                businessId.Value,
                sensorIds,
                cached.From,
                cached.To,
                new ConfigurationContext { Features = cached.Features },
                allowedOfficeIds,
                includeBusinessLevel: allowedOfficeIds is null && level == "business");
            cached.Summary.SavingOpportunityKwh = Round(
                cached.Suggestions.Sum(x => x.EstimatedSavingKwh ?? 0));
            cached.Summary.SavingOpportunityCost = cached.Features.SavingsCostAnalysis
                ? Round(cached.Suggestions.Sum(x => x.EstimatedSavingCost ?? 0))
                : null;
        }

        if (cached.Children.Count == 0)
            return;

        var mappings = await LoadChildSensorMappingsAsync(level, id, allowedOfficeIds);
        var sensorsByChild = mappings
            .GroupBy(x => x.ChildId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.SensorId).Distinct().ToHashSet());
        var issuesBySensor = health.Issues.ToLookup(x => x.SensorId);

        foreach (var child in cached.Children)
        {
            var childSensors = sensorsByChild.GetValueOrDefault(child.Id) ?? new HashSet<Guid>();
            child.OnlineSensorCount = childSensors.Count(health.OnlineSensorIds.Contains);
            child.IssueCount = childSensors.Sum(sensorId => issuesBySensor[sensorId].Count());
            child.Status = child.IssueCount > 0 || child.ChangePercent > 15
                ? "High"
                : child.ChangePercent > 5 ? "Review" : "Normal";
        }
    }

    private async Task AppendLatestRangeAsync(
        string level,
        Guid id,
        string rangeKey,
        DateTime aggregateTo,
        DateTime responseTo,
        DeepDiveResponseDto response,
        string requestedTimeZone,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        if (responseTo <= aggregateTo)
            return;

        var sensorIds = await ResolveSensorIdsAsync(level, id, allowedOfficeIds);
        response.To = responseTo;
        if (sensorIds.Count == 0)
            return;

        var businessId = await ResolveBusinessIdAsync(level, id);
        var timeZone = string.IsNullOrWhiteSpace(requestedTimeZone)
            ? response.Timezone
            : requestedTimeZone;
        var config = businessId.HasValue
            ? await LoadConfigurationAsync(businessId.Value, sensorIds, level, id, timeZone)
            : new ConfigurationContext { TimeZoneId = response.Timezone };

        var tailRows = await LoadRowsAsync(sensorIds, aggregateTo, responseTo);
        if (tailRows.Count > 0)
        {
            var tailEnergy = CalculateEnergy(tailRows);
            response.Summary.EnergyKwh = Round(response.Summary.EnergyKwh + tailEnergy);
            response.Summary.EnergyChangePercent = ChangePercent(
                response.Summary.EnergyKwh,
                response.Summary.PreviousEnergyKwh);

            if (response.Summary.EstimatedCost.HasValue && config.Features.CostAnalysis)
            {
                response.Summary.EstimatedCost = Round(
                    response.Summary.EstimatedCost.Value + CalculateCost(tailRows, config));
                response.Summary.CostChangePercent =
                    response.Summary.PreviousEstimatedCost.HasValue
                        ? ChangePercent(
                            response.Summary.EstimatedCost.Value,
                            response.Summary.PreviousEstimatedCost.Value)
                        : null;
            }

            var tailPeakOffPeak = CalculatePeakOffPeak(tailRows, config);
            if (response.PeakOffPeak.IsAvailable && tailPeakOffPeak.IsAvailable)
            {
                response.PeakOffPeak.PeakEnergyKwh = Round(
                    response.PeakOffPeak.PeakEnergyKwh + tailPeakOffPeak.PeakEnergyKwh);
                response.PeakOffPeak.OffPeakEnergyKwh = Round(
                    response.PeakOffPeak.OffPeakEnergyKwh + tailPeakOffPeak.OffPeakEnergyKwh);
                if (response.PeakOffPeak.PeakCost.HasValue && tailPeakOffPeak.PeakCost.HasValue)
                    response.PeakOffPeak.PeakCost = Round(
                        response.PeakOffPeak.PeakCost.Value + tailPeakOffPeak.PeakCost.Value);
                if (response.PeakOffPeak.OffPeakCost.HasValue && tailPeakOffPeak.OffPeakCost.HasValue)
                    response.PeakOffPeak.OffPeakCost = Round(
                        response.PeakOffPeak.OffPeakCost.Value + tailPeakOffPeak.OffPeakCost.Value);

                var total = response.PeakOffPeak.PeakEnergyKwh +
                            response.PeakOffPeak.OffPeakEnergyKwh;
                response.PeakOffPeak.PeakSharePercent = total > 0
                    ? Round(response.PeakOffPeak.PeakEnergyKwh * 100 / total)
                    : 0;
                response.PeakOffPeak.OffPeakSharePercent = total > 0
                    ? Round(response.PeakOffPeak.OffPeakEnergyKwh * 100 / total)
                    : 0;
            }

            var tailDemand = BuildDemand(tailRows, config);
            var baseMinutes = Math.Max(1, (aggregateTo - response.From).TotalMinutes);
            var tailMinutes = Math.Max(1, (responseTo - aggregateTo).TotalMinutes);
            response.Demand.AverageDemandKw = Round(
                ((response.Demand.AverageDemandKw * baseMinutes) +
                 (tailDemand.AverageDemandKw * tailMinutes)) /
                (baseMinutes + tailMinutes));

            if (tailDemand.PeakDemandKw > response.Demand.PeakDemandKw)
            {
                response.Demand.PeakDemandKw = tailDemand.PeakDemandKw;
                response.Demand.PeakDemandAt = tailDemand.PeakDemandAt;
            }
            if (response.Demand.BreachCount.HasValue && tailDemand.BreachCount.HasValue)
                response.Demand.BreachCount += tailDemand.BreachCount.Value;
            if (response.Demand.MinutesAboveThreshold.HasValue &&
                tailDemand.MinutesAboveThreshold.HasValue)
                response.Demand.MinutesAboveThreshold += tailDemand.MinutesAboveThreshold.Value;

            response.Summary.PeakDemandKw = response.Demand.PeakDemandKw;
            response.Summary.PeakDemandAt = response.Demand.PeakDemandAt;

            response.DataStatus.HasReadings = true;
            response.DataStatus.SensorsWithReadings = Math.Max(
                response.DataStatus.SensorsWithReadings,
                tailRows.Where(x => x.SampleCount > 0)
                    .Select(x => x.SensorId)
                    .Distinct()
                    .Count());
            var tailFirst = tailRows.Min(x => x.FirstReadingAt);
            var tailLast = tailRows.Max(x => x.LastReadingAt);
            if (!response.DataStatus.FirstReadingAt.HasValue ||
                tailFirst < response.DataStatus.FirstReadingAt.Value)
                response.DataStatus.FirstReadingAt = tailFirst;
            if (!response.DataStatus.LastReadingAt.HasValue ||
                tailLast > response.DataStatus.LastReadingAt.Value)
                response.DataStatus.LastReadingAt = tailLast;

            await MergeChildrenWithTailAsync(level, id, response, tailRows, config, allowedOfficeIds);
            MergeTrendWithTail(response, tailRows, config, rangeKey, aggregateTo);
            MergeMainChartsWithTail(response, tailRows, config, rangeKey, responseTo);
        }
        else
        {
            response.CrmCharts.EnergyConsumption.toDate = responseTo.ToString("o");
            response.CrmCharts.PeakNonPeak.toDate = responseTo.ToString("o");
            response.CrmCharts.HighDemand.toDate = responseTo.ToString("o");
        }

        // Utility trend stays a fixed month-wise last-12-month chart for every
        // selected Deep Dive range. Only today's missing portion is added here.
        var utilityTrendTailFrom = ParseChartTime(
            response.CrmCharts.UtilityTrend.toDate,
            ResolveLocalBucketStartUtc(aggregateTo, "day", config.TimeZoneId));
        if (utilityTrendTailFrom < responseTo)
        {
            var rows = utilityTrendTailFrom == aggregateTo
                ? tailRows
                : await LoadRowsAsync(sensorIds, utilityTrendTailFrom, responseTo);
            MergeMonthlyUtilityTrendWithTail(
                response.CrmCharts.UtilityTrend, rows, responseTo, config.TimeZoneId);
        }

        var utilityMixTailFrom = ParseChartTime(
            response.CrmCharts.UtilityMix.toDate,
            aggregateTo);
        if (utilityMixTailFrom < responseTo)
        {
            var rows = utilityMixTailFrom == aggregateTo
                ? tailRows
                : await LoadRowsAsync(sensorIds, utilityMixTailFrom, responseTo);
            MergeUtilityMixWithTail(response.CrmCharts.UtilityMix, rows, responseTo);
        }

        response.Insight = BuildInsight(
            response.Children,
            response.Summary.EnergyKwh,
            response.Configuration);
    }

    private async Task MergeChildrenWithTailAsync(
        string level,
        Guid id,
        DeepDiveResponseDto response,
        List<ReadingRow> tailRows,
        ConfigurationContext config,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        if (response.Children.Count == 0 || tailRows.Count == 0)
            return;

        var mappings = await LoadChildSensorMappingsAsync(level, id, allowedOfficeIds);
        var sensorIdsByChild = mappings
            .GroupBy(x => x.ChildId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.SensorId).Distinct().ToHashSet());
        var rowsBySensor = tailRows.ToLookup(x => x.SensorId);

        foreach (var child in response.Children)
        {
            var sensorIds = sensorIdsByChild.GetValueOrDefault(child.Id);
            if (sensorIds is null || sensorIds.Count == 0)
                continue;

            var rows = sensorIds.SelectMany(sensorId => rowsBySensor[sensorId]).ToList();
            if (rows.Count == 0)
                continue;

            child.EnergyKwh = Round(child.EnergyKwh + CalculateEnergy(rows));
            if (child.EstimatedCost.HasValue && config.Features.CostAnalysis)
                child.EstimatedCost = Round(
                    child.EstimatedCost.Value + CalculateCost(rows, config));

            var demand = BuildDemand(rows, config);
            child.PeakDemandKw = Math.Max(child.PeakDemandKw, demand.PeakDemandKw);
            child.ChangePercent = ChangePercent(child.EnergyKwh, child.PreviousEnergyKwh);
            child.Status = child.IssueCount > 0 || child.ChangePercent > 15
                ? "High"
                : child.ChangePercent > 5 ? "Review" : "Normal";
        }

        foreach (var child in response.Children)
        {
            child.SharePercent = response.Summary.EnergyKwh > 0
                ? Round(child.EnergyKwh * 100 / response.Summary.EnergyKwh)
                : 0;
        }

        response.Children = response.Children
            .OrderByDescending(x => x.EnergyKwh)
            .ToList();
    }

    private static void MergeTrendWithTail(
        DeepDiveResponseDto response,
        List<ReadingRow> tailRows,
        ConfigurationContext config,
        string rangeKey,
        DateTime aggregateTo)
    {
        if (tailRows.Count == 0)
            return;

        var span = aggregateTo - response.From;
        var granularity = span.TotalDays > 2 ? "day" : "hour";
        Func<DateTime, DateTime> bucket = value =>
            ResolveLocalBucketStartUtc(value, granularity, config.TimeZoneId);

        foreach (var group in tailRows.GroupBy(x => bucket(x.At)))
        {
            var key = group.Key;
            var rows = group.ToList();
            var energy = CalculateEnergy(rows);
            var demand = rows.GroupBy(x => x.SensorId)
                .Sum(WeightedAverageActivePower) / 1000.0;
            var cost = config.Features.CostAnalysis
                ? Round(CalculateCost(rows, config))
                : (double?)null;

            var point = response.Trend.FirstOrDefault(x => x.Bucket == key);
            if (point is null)
            {
                var local = ConvertToConfiguredTime(key, config.TimeZoneId);
                point = new DeepDiveTrendPointDto
                {
                    Bucket = key,
                    Label = granularity == "day"
                        ? local.ToString("dd MMM yyyy")
                        : local.ToString("dd MMM HH:mm"),
                    PreviousEnergyKwh = 0,
                    PreviousDemandKw = 0,
                    PreviousCost = null
                };
                response.Trend.Add(point);
            }

            point.EnergyKwh = Round(point.EnergyKwh + energy);
            point.DemandKw = Round(Math.Max(point.DemandKw, demand));
            if (point.Cost.HasValue && cost.HasValue)
                point.Cost = Round(point.Cost.Value + cost.Value);
            else if (!point.Cost.HasValue && cost.HasValue)
                point.Cost = cost;
        }

        response.Trend = response.Trend.OrderBy(x => x.Bucket).ToList();
    }

    private static void MergeMainChartsWithTail(
        DeepDiveResponseDto response,
        List<ReadingRow> tailRows,
        ConfigurationContext config,
        string rangeKey,
        DateTime responseTo)
    {
        MergeEnergyChartWithTail(
            response.CrmCharts.EnergyConsumption,
            tailRows,
            response.From,
            responseTo,
            rangeKey,
            config.TimeZoneId);
        MergePeakChartWithTail(
            response.CrmCharts.PeakNonPeak,
            tailRows,
            response.From,
            responseTo,
            rangeKey,
            config);
        MergeDemandChartWithTail(
            response.CrmCharts.HighDemand,
            tailRows,
            response.From,
            responseTo,
            rangeKey,
            config);
    }

    private static void MergeEnergyChartWithTail(
        CrmDashboardChartResponseDTO chart,
        List<ReadingRow> rows,
        DateTime from,
        DateTime to,
        string rangeKey,
        string timeZoneId)
    {
        var bucket = ResolveCrmBucket(from, to, timeZoneId);
        foreach (var group in rows.Where(x => x.EnergyKwh > 0).GroupBy(x => bucket(x.At)))
        {
            var point = FindOrAddChartPoint(chart, group.Key, rangeKey, timeZoneId);
            point.totalKwh = Round(point.totalKwh + group.Sum(x => x.EnergyKwh));
            point.value = point.totalKwh;
        }

        RebuildSingleSeriesChart(chart, "Energy Consumption", x => x.totalKwh);
        chart.totalKwh = Round(chart.points.Sum(x => x.totalKwh));
        chart.toDate = to.ToString("o");
    }

    private static void MergePeakChartWithTail(
        CrmDashboardChartResponseDTO chart,
        List<ReadingRow> rows,
        DateTime from,
        DateTime to,
        string rangeKey,
        ConfigurationContext config)
    {
        if (!config.Features.PeakOffPeakAnalysis)
        {
            chart.toDate = to.ToString("o");
            return;
        }

        var bucket = ResolveCrmBucket(from, to, config.TimeZoneId);
        foreach (var group in CalculateEnergyIntervals(rows).GroupBy(x => bucket(x.At)))
        {
            var point = FindOrAddChartPoint(chart, group.Key, rangeKey, config.TimeZoneId);
            point.peakKwh = Round(point.peakKwh +
                group.Where(x => IsPeak(x.At, config)).Sum(x => x.EnergyKwh));
            point.nonPeakKwh = Round(point.nonPeakKwh +
                group.Where(x => !IsPeak(x.At, config)).Sum(x => x.EnergyKwh));
            point.totalKwh = Round(point.peakKwh + point.nonPeakKwh);
            point.value = point.totalKwh;
        }

        chart.points = SortChartPoints(chart.points);
        chart.categories = chart.points.Select(x => x.label).ToList();
        EnsureSeries(chart, 2);
        chart.series[0].name = "Peak kWh";
        chart.series[0].data = chart.points.Select(x => x.peakKwh).ToList();
        chart.series[1].name = "Non-Peak kWh";
        chart.series[1].data = chart.points.Select(x => x.nonPeakKwh).ToList();
        chart.totalPeakKwh = Round(chart.points.Sum(x => x.peakKwh));
        chart.totalNonPeakKwh = Round(chart.points.Sum(x => x.nonPeakKwh));
        chart.totalKwh = Round(chart.totalPeakKwh + chart.totalNonPeakKwh);
        chart.toDate = to.ToString("o");
    }

    private static void MergeDemandChartWithTail(
        CrmDashboardChartResponseDTO chart,
        List<ReadingRow> rows,
        DateTime from,
        DateTime to,
        string rangeKey,
        ConfigurationContext config)
    {
        var interval = Math.Clamp(Math.Max(15, config.DemandIntervalMinutes), 15, 60);
        var bucket = ResolveCrmBucket(from, to, config.TimeZoneId);
        var demandByBucket = rows
            .GroupBy(x =>
            {
                var minute = x.At.Minute - (x.At.Minute % interval);
                return new DateTime(
                    x.At.Year, x.At.Month, x.At.Day, x.At.Hour, minute, 0, DateTimeKind.Utc);
            })
            .Select(g => new
            {
                At = g.Key,
                DemandW = g.GroupBy(x => x.SensorId).Sum(WeightedAverageActivePower)
            })
            .GroupBy(x => bucket(x.At))
            .ToDictionary(x => x.Key, x => x.Max(y => y.DemandW));

        foreach (var item in demandByBucket)
        {
            var point = FindOrAddChartPoint(chart, item.Key, rangeKey, config.TimeZoneId);
            point.demandW = Round(Math.Max(point.demandW, item.Value));
            point.value = point.demandW;
        }

        RebuildSingleSeriesChart(chart, "Peak Demand", x => x.demandW);
        chart.peakDemandW = chart.points.Count == 0
            ? 0
            : Round(chart.points.Max(x => x.demandW));
        chart.toDate = to.ToString("o");
    }

    private static CrmDashboardChartPointDTO FindOrAddChartPoint(
        CrmDashboardChartResponseDTO chart,
        DateTime key,
        string rangeKey,
        string timeZoneId)
    {
        chart.points ??= new List<CrmDashboardChartPointDTO>();
        var point = chart.points.FirstOrDefault(x =>
            TryParseChartTime(x.period, out var at) && at == key);
        if (point is not null)
            return point;

        point = new CrmDashboardChartPointDTO
        {
            label = FormatCrmBucketLabel(key, rangeKey, timeZoneId),
            period = key.ToString("o")
        };
        chart.points.Add(point);
        return point;
    }

    private static void RebuildSingleSeriesChart(
        CrmDashboardChartResponseDTO chart,
        string seriesName,
        Func<CrmDashboardChartPointDTO, double> valueSelector)
    {
        chart.points = SortChartPoints(chart.points);
        chart.categories = chart.points.Select(x => x.label).ToList();
        EnsureSeries(chart, 1);
        chart.series[0].name = seriesName;
        chart.series[0].data = chart.points.Select(valueSelector).ToList();
    }

    private static void EnsureSeries(CrmDashboardChartResponseDTO chart, int count)
    {
        chart.series ??= new List<CrmDashboardChartSeriesDTO>();
        while (chart.series.Count < count)
            chart.series.Add(new CrmDashboardChartSeriesDTO());
    }

    private static List<CrmDashboardChartPointDTO> SortChartPoints(
        IEnumerable<CrmDashboardChartPointDTO>? points) =>
        (points ?? Enumerable.Empty<CrmDashboardChartPointDTO>())
            .OrderBy(x => TryParseChartTime(x.period, out var at) ? at : DateTime.MaxValue)
            .ToList();

    private static void MergeMonthlyUtilityTrendWithTail(
        CrmDashboardChartResponseDTO chart,
        List<ReadingRow> rows,
        DateTime responseTo,
        string timeZoneId)
    {
        chart.points ??= new List<CrmDashboardChartPointDTO>();
        chart.series ??= new List<CrmDashboardChartSeriesDTO>();

        var monthKeys = chart.points
            .Select(x => TryParseChartTime(x.period, out var at) ? at : (DateTime?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        var values = new Dictionary<(DateTime Month, string Utility), double>();

        for (var seriesIndex = 0; seriesIndex < chart.series.Count; seriesIndex++)
        {
            var series = chart.series[seriesIndex];
            for (var pointIndex = 0;
                 pointIndex < monthKeys.Count && pointIndex < series.data.Count;
                 pointIndex++)
            {
                values[(monthKeys[pointIndex], series.name)] = series.data[pointIndex];
            }
        }

        foreach (var group in CalculateEnergyIntervals(rows).GroupBy(x => new
                 {
                     Month = ResolveLocalBucketStartUtc(x.At, "month", timeZoneId),
                     Utility = NormalizedUtilityName(x.UtilityName)
                 }))
        {
            var key = (group.Key.Month, group.Key.Utility);
            values[key] = Round(values.GetValueOrDefault(key) + group.Sum(x => x.EnergyKwh));
            if (!monthKeys.Contains(group.Key.Month))
                monthKeys.Add(group.Key.Month);
        }

        monthKeys = monthKeys.Distinct().OrderBy(x => x).TakeLast(12).ToList();
        var utilities = values.Keys.Select(x => x.Utility)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        chart.categories = monthKeys
            .Select(x => ConvertToConfiguredTime(x, timeZoneId).ToString("MMM yyyy"))
            .ToList();
        chart.series = utilities.Select(utility => new CrmDashboardChartSeriesDTO
        {
            name = utility,
            data = monthKeys.Select(month => Round(values.GetValueOrDefault((month, utility)))).ToList()
        }).ToList();
        chart.points = monthKeys.Select((month, index) => new CrmDashboardChartPointDTO
        {
            label = chart.categories[index],
            period = month.ToString("o"),
            value = Round(utilities.Sum(utility => values.GetValueOrDefault((month, utility)))),
            totalKwh = Round(utilities.Sum(utility => values.GetValueOrDefault((month, utility))))
        }).ToList();
        chart.totalKwh = Round(chart.series.Sum(x => x.data.Sum()));
        chart.fromDate = monthKeys.Count > 0 ? monthKeys[0].ToString("o") : chart.fromDate;
        chart.toDate = responseTo.ToString("o");
        chart.range = "1y";
    }

    private static void MergeUtilityMixWithTail(
        CrmDashboardChartResponseDTO chart,
        List<ReadingRow> rows,
        DateTime responseTo)
    {
        chart.categories ??= new List<string>();
        chart.series ??= new List<CrmDashboardChartSeriesDTO>();
        var totals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        if (chart.series.Count > 0)
        {
            for (var index = 0;
                 index < chart.categories.Count && index < chart.series[0].data.Count;
                 index++)
            {
                totals[chart.categories[index]] = chart.series[0].data[index];
            }
        }

        foreach (var group in CalculateEnergyIntervals(rows)
                     .GroupBy(x => NormalizedUtilityName(x.UtilityName),
                         StringComparer.OrdinalIgnoreCase))
        {
            totals[group.Key] = Round(
                totals.GetValueOrDefault(group.Key) + group.Sum(x => x.EnergyKwh));
        }

        var ordered = totals.OrderByDescending(x => x.Value).ToList();
        chart.categories = ordered.Select(x => x.Key).ToList();
        EnsureSeries(chart, 1);
        chart.series[0].name = "Utility Mix";
        chart.series[0].data = ordered.Select(x => Round(x.Value)).ToList();
        chart.points = ordered.Select(x => new CrmDashboardChartPointDTO
        {
            label = x.Key,
            period = responseTo.ToString("o"),
            value = Round(x.Value),
            totalKwh = Round(x.Value)
        }).ToList();
        chart.totalKwh = Round(ordered.Sum(x => x.Value));
        chart.toDate = responseTo.ToString("o");
    }

    private static DateTime ParseChartTime(string? value, DateTime fallback) =>
        TryParseChartTime(value, out var parsed) ? parsed : EnsureUtc(fallback);

    private static bool TryParseChartTime(string? value, out DateTime parsed)
    {
        if (DateTime.TryParse(
                value,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var result))
        {
            parsed = EnsureUtc(result);
            return true;
        }

        parsed = default;
        return false;
    }

    private async Task<List<ReadingRow>> LoadRowsAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var buckets = await _energyAggregateStore.LoadAsync(
            sensorIds, from, to, cancellationToken);

        return buckets.Select(x => new ReadingRow
        {
            SensorId = x.SensorId,
            UtilityName = x.UtilityName,
            At = x.At,
            EnergyKwh = x.EnergyKwh,
            ActivePower = x.ActivePower,
            MaxActivePower = x.MaxActivePower,
            Voltage = x.Voltage,
            PowerFactor = x.PowerFactor,
            SampleCount = x.SampleCount,
            FirstReadingAt = x.FirstReadingAt,
            LastReadingAt = x.LastReadingAt,
            ResetCount = x.ResetCount,
            IgnoredSpikeCount = x.IgnoredSpikeCount
        }).ToList();
    }

    private async Task<ConfigurationContext> LoadConfigurationAsync(
        Guid businessId,
        List<Guid> sensorIds,
        string level,
        Guid entityId,
        string requestedTimeZone)
    {
        var plan = await _db.tbl_energy_tariff_plan.AsNoTracking()
            .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
            .OrderByDescending(x => x.updated_at)
            .FirstOrDefaultAsync();

        var periods = plan is null
            ? new List<TariffWindow>()
            : await _db.tbl_tariff_time_period.AsNoTracking()
                .Where(x => x.fk_tariff_plan == plan.energy_tariff_plan_id && x.is_active && !x.is_deleted)
                .Select(x => new TariffWindow
                {
                    Type = x.period_type,
                    Start = x.start_time,
                    End = x.end_time,
                    DayOfWeek = x.day_of_week,
                    SeasonStart = x.season_start,
                    SeasonEnd = x.season_end
                }).ToListAsync();

        var legacy = await _db.tbl_business_dashboard_setting.AsNoTracking()
            .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
            .OrderByDescending(x => x.updated_at)
            .FirstOrDefaultAsync();

        var demand = await _db.tbl_demand_management_setting.AsNoTracking()
            .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
            .OrderByDescending(x => x.updated_at)
            .FirstOrDefaultAsync();

        var assignments = sensorIds.Count == 0
            ? new List<ApplianceProfileRow>()
            : await _db.tbl_sensor_appliance.AsNoTracking()
                .Where(x => sensorIds.Contains(x.fk_sensor) && x.is_active && !x.is_deleted &&
                            !x.appliance.is_deleted && x.appliance.is_active)
                .Select(x => new ApplianceProfileRow
                {
                    SensorId = x.fk_sensor,
                    MaxPower = x.appliance.max_power,
                    StandbyPower = x.appliance.standby_power,
                    NormalPowerFactor = x.appliance.normal_power_factor
                }).ToListAsync();

        var officeRows = sensorIds.Count == 0
            ? new List<OfficeScheduleRow>()
            : await _db.tbl_sensor.AsNoTracking()
                .Where(x => sensorIds.Contains(x.sensor_id))
                .Select(x => new OfficeScheduleRow
                {
                    Is24Hours = x.device.office.is_24_hours,
                    WorkingDays = x.device.office.working_days,
                    OpeningTime = x.device.office.opening_time,
                    ClosingTime = x.device.office.closing_time
                })
                .Distinct()
                .ToListAsync();
        var officeCount = officeRows.Count;
        var officeScheduleCount = officeRows.Count(x =>
            x.Is24Hours ||
            (!string.IsNullOrWhiteSpace(x.WorkingDays) && x.OpeningTime != x.ClosingTime));

        requestedTimeZone = string.IsNullOrWhiteSpace(requestedTimeZone)
            ? "UTC"
            : requestedTimeZone.Trim();
        var hasValidTimezone = IsValidTimeZone(requestedTimeZone);
        var appliedTimeZone = hasValidTimezone ? requestedTimeZone : "UTC";

        var hasActivePlan = plan is not null;
        var standardRate = (double)(plan?.standard_rate_per_kwh ?? legacy?.tariff_rate ?? 0);
        var peakRate = (double)(plan?.peak_rate_per_kwh ?? legacy?.peak_tariff_rate ?? 0);
        var offPeakRate = (double)(plan?.off_peak_rate_per_kwh ?? legacy?.off_peak_tariff_rate ?? 0);
        var hasStandardRate = standardRate > 0;
        var hasTimeOfUseRates = peakRate > 0 && offPeakRate > 0;
        var hasRates = hasStandardRate || hasTimeOfUseRates;

        if (periods.Count == 0 && legacy is not null &&
            TimeOnly.TryParse(legacy.peak_start_time, out var legacyStart) &&
            TimeOnly.TryParse(legacy.peak_end_time, out var legacyEnd))
        {
            periods.Add(new TariffWindow
            {
                Type = "Peak",
                Start = legacyStart,
                End = legacyEnd,
                DayOfWeek = legacy.day_of_week
            });
        }

        var hasPeakSchedule = periods.Any(x =>
            x.Type.Equals("Peak", StringComparison.OrdinalIgnoreCase));
        var peakOffPeakReady = hasPeakSchedule && hasValidTimezone;
        var hasCostConfiguration = hasStandardRate || (peakOffPeakReady && hasTimeOfUseRates);
        var hasDemand = demand is not null;
        var hasDemandLimit = demand is not null && demand.demand_limit_kw > 0;
        var sensorsWithAppliance = assignments.Select(x => x.SensorId).Distinct().Count();

        // Removed appliance optimization toggles are no longer required. A valid
        // assigned appliance with electrical thresholds is sufficient for analysis.
        var sensorsWithProfile = assignments
            .Where(x => x.MaxPower > 0 && x.NormalPowerFactor > 0)
            .Select(x => x.SensorId)
            .Distinct()
            .Count();
        var coverage = sensorIds.Count > 0
            ? Round(sensorsWithProfile * 100.0 / sensorIds.Count)
            : 0;
        var hasAssignments = sensorIds.Count > 0 && sensorsWithAppliance > 0;
        var hasThresholds = sensorIds.Count > 0 && sensorsWithProfile > 0;
        var hasCompleteProfiles = sensorIds.Count > 0 && sensorsWithProfile == sensorIds.Count;
        var hasOfficeSchedules = officeCount == 0 || officeScheduleCount == officeCount;

        var requirements = new List<DeepDiveConfigurationRequirementDto>
        {
            Requirement(
                "tariff-plan",
                "Energy tariff",
                hasActivePlan && hasCostConfiguration
                    ? "The active tariff can be used for cost calculations."
                    : hasCostConfiguration
                        ? "Legacy rates are available; save the tariff form when convenient."
                        : "Add a standard rate, or peak/off-peak rates with a schedule, before showing cost results.",
                hasActivePlan && hasCostConfiguration ? "ready" : hasCostConfiguration ? "partial" : "missing",
                EnergyConfigurationRoute),
            Requirement(
                "peak-periods",
                "Peak/off-peak schedule",
                hasPeakSchedule
                    ? "At least one active peak period is configured."
                    : "Configure peak hours before splitting energy and cost into peak and off-peak periods.",
                hasPeakSchedule ? "ready" : "missing",
                EnergyConfigurationRoute),
            Requirement(
                "reporting-timezone",
                "Reporting timezone",
                hasValidTimezone
                    ? $"The confirmed reporting timezone is {appliedTimeZone}."
                    : "The requested timezone is invalid; UTC has been applied.",
                hasValidTimezone ? "ready" : "partial",
                string.Empty),
            Requirement(
                "demand-settings",
                "Demand management",
                hasDemandLimit
                    ? "Demand interval and limit are available for threshold analysis."
                    : "Set a demand limit and interval before showing breach counts.",
                hasDemandLimit ? "ready" : "missing",
                EnergyConfigurationRoute),
            Requirement(
                "appliance-profiles",
                "Appliance assignment and limits",
                sensorIds.Count == 0
                    ? "No sensors exist in this scope."
                    : $"{sensorsWithProfile} of {sensorIds.Count} sensors have an appliance with usable electrical limits.",
                sensorIds.Count == 0 ? "missing" : hasCompleteProfiles ? "ready" : sensorsWithProfile > 0 ? "partial" : "missing",
                "/core/sensor-management"),
            Requirement(
                "office-schedules",
                "Office operating schedules",
                hasOfficeSchedules
                    ? "Office schedules are available for after-hours analysis."
                    : "Complete office working days and opening/closing times for after-hours analysis.",
                hasOfficeSchedules ? "ready" : "partial",
                "/core/office-management")
        };

        var status = new DeepDiveConfigurationStatusDto
        {
            IsReadyForOptimization = hasCostConfiguration && peakOffPeakReady && hasDemandLimit &&
                                     hasCompleteProfiles && hasOfficeSchedules,
            HasActiveTariffPlan = hasActivePlan,
            HasTariffRates = hasRates,
            HasPeakSchedule = hasPeakSchedule,
            HasValidTimezone = hasValidTimezone,
            HasDemandSettings = hasDemand,
            HasDemandLimit = hasDemandLimit,
            HasOfficeSchedules = hasOfficeSchedules,
            HasApplianceAssignments = hasAssignments,
            HasApplianceThresholds = hasThresholds,
            TotalSensors = sensorIds.Count,
            SensorsWithAppliance = sensorsWithAppliance,
            SensorsWithOptimizationProfile = sensorsWithProfile,
            ApplianceCoveragePercent = coverage,
            Requirements = requirements
        };

        return new ConfigurationContext
        {
            Currency = plan?.currency ?? legacy?.currency ?? "PKR",
            TimeZoneId = appliedTimeZone,
            StandardRate = standardRate,
            PeakRate = peakRate > 0 ? peakRate : standardRate,
            OffPeakRate = offPeakRate > 0 ? offPeakRate : standardRate,
            TariffWindows = periods,
            DemandIntervalMinutes = demand?.demand_interval_minutes is >= 15 and <= 60
                ? demand.demand_interval_minutes
                : 15,
            DemandLimitKw = hasDemandLimit ? (double)demand!.demand_limit_kw : null,
            WarningThresholdPercent = hasDemand ? (double)demand!.warning_threshold_percent : 90,
            OnlineThresholdSeconds = legacy is not null && legacy.online_sensor_threshold_seconds > 0
                ? legacy.online_sensor_threshold_seconds
                : 120,
            Status = status,
            Features = new DeepDiveFeatureAvailabilityDto
            {
                EnergyAnalysis = true,
                CostAnalysis = hasCostConfiguration,
                PeakOffPeakAnalysis = peakOffPeakReady,
                DemandAnalysis = true,
                DemandThresholdAnalysis = hasDemandLimit,
                OptimizationSuggestions = hasThresholds,
                SavingsCostAnalysis = hasCostConfiguration && hasThresholds
            }
        };
    }

    private IQueryable<tbl_office> ScopeOffices(string level, Guid id)
    {
        return level switch
        {
            "business" => _db.tbl_office.Where(x => !x.is_deleted && x.fk_business == id),
            "facility" => _db.tbl_office.Where(x => !x.is_deleted && x.section.floor.building.fk_facility == id),
            "building" => _db.tbl_office.Where(x => !x.is_deleted && x.section.floor.fk_building == id),
            "floor" => _db.tbl_office.Where(x => !x.is_deleted && x.section.fk_floor == id),
            "section" => _db.tbl_office.Where(x => !x.is_deleted && x.fk_section == id),
            "office" => _db.tbl_office.Where(x => !x.is_deleted && x.office_id == id),
            "device" => _db.tbl_office.Where(x => !x.is_deleted && x.devices.Any(d => d.device_id == id)),
            "sensor" => _db.tbl_office.Where(x => !x.is_deleted && x.devices.Any(d => d.sensors.Any(s => s.sensor_id == id))),
            _ => _db.tbl_office.Where(x => false)
        };
    }

    private static DeepDiveConfigurationRequirementDto Requirement(
        string key, string title, string description, string status, string route) => new()
    {
        Key = key,
        Title = title,
        Description = description,
        Status = status,
        Route = route
    };

    private static double CalculateEnergy(IEnumerable<ReadingRow> rows) =>
        rows.Where(x => x.EnergyKwh > 0).Sum(x => x.EnergyKwh);

    private static DeepDivePeakOffPeakDto CalculatePeakOffPeak(
        List<ReadingRow> rows,
        ConfigurationContext config)
    {
        if (!config.Features.PeakOffPeakAnalysis)
        {
            return new DeepDivePeakOffPeakDto
            {
                IsAvailable = false,
                UnavailableReason = "Peak/off-peak periods are not configured."
            };
        }

        double peak = 0;
        double offPeak = 0;
        double peakCost = 0;
        double offPeakCost = 0;

        foreach (var row in rows.Where(x => x.EnergyKwh > 0))
        {
            var isPeak = IsPeak(row.At, config);
            if (isPeak)
            {
                peak += row.EnergyKwh;
                if (config.Features.CostAnalysis)
                    peakCost += row.EnergyKwh * ResolveRate(row.At, config);
            }
            else
            {
                offPeak += row.EnergyKwh;
                if (config.Features.CostAnalysis)
                    offPeakCost += row.EnergyKwh * ResolveRate(row.At, config);
            }
        }

        var total = peak + offPeak;
        return new DeepDivePeakOffPeakDto
        {
            IsAvailable = true,
            PeakEnergyKwh = Round(peak),
            OffPeakEnergyKwh = Round(offPeak),
            PeakSharePercent = total > 0 ? Round(peak * 100 / total) : 0,
            OffPeakSharePercent = total > 0 ? Round(offPeak * 100 / total) : 0,
            PeakCost = config.Features.CostAnalysis ? Round(peakCost) : null,
            OffPeakCost = config.Features.CostAnalysis ? Round(offPeakCost) : null
        };
    }

    private async Task<DeepDiveCrmChartsDto> BuildCrmChartsAsync(
        string level,
        Guid id,
        Guid businessId,
        List<Guid> sensorIds,
        List<ReadingRow> currentRows,
        DateTime from,
        DateTime to,
        ConfigurationContext config,
        DeepDiveQueryDto query)
    {
        var rangeKey = NormalizeRangeKey(query);

        // This chart is intentionally independent of the selected Deep Dive range:
        // always month-wise for the latest 12 local calendar months.
        var utilityTrendTo = ResolveLocalBucketStartUtc(to, "day", config.TimeZoneId);
        var localTrendTo = ConvertToConfiguredTime(utilityTrendTo, config.TimeZoneId);
        var localFirstMonth = new DateTime(
            localTrendTo.Year,
            localTrendTo.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Unspecified).AddMonths(-11);
        var utilityTrendFrom = ConvertLocalToUtc(localFirstMonth, config.TimeZoneId);

        // Utility mix remains the latest 30 completed days/hours and receives only
        // the small current live edge in AppendLatestRangeAsync.
        var utilityMixTo = new DateTime(
            to.Year, to.Month, to.Day, to.Hour, 0, 0, DateTimeKind.Utc);
        var utilityMixFrom = utilityMixTo.AddDays(-30);

        var utilityTrend = await TryReadChartAggregateAsync<CrmDashboardChartResponseDTO>(
            level, id, "deepdive-utilitytrend", "1y", utilityTrendFrom, utilityTrendTo);
        if (utilityTrend is null)
        {
            var rows = rangeKey == "1y" && from == utilityTrendFrom && to == utilityTrendTo
                ? currentRows
                : await LoadRowsAsync(sensorIds, utilityTrendFrom, utilityTrendTo);
            utilityTrend = BuildUtilityTrendChart(rows, utilityTrendFrom, utilityTrendTo, config.TimeZoneId);
            await UpsertAggregateAsync(level, id, businessId, "1y", utilityTrendFrom, utilityTrendTo,
                "deepdive-utilitytrend", utilityTrend);
        }

        var utilityMix = await TryReadChartAggregateAsync<CrmDashboardChartResponseDTO>(
            level, id, "deepdive-utilitymix", "30d", utilityMixFrom, utilityMixTo);
        if (utilityMix is null)
        {
            var rows = rangeKey == "30d" && from == utilityMixFrom && to == utilityMixTo
                ? currentRows
                : await LoadRowsAsync(sensorIds, utilityMixFrom, utilityMixTo);
            utilityMix = BuildUtilityMixChart(rows, utilityMixFrom, utilityMixTo);
            await UpsertAggregateAsync(level, id, businessId, "30d", utilityMixFrom, utilityMixTo,
                "deepdive-utilitymix", utilityMix);
        }

        return new DeepDiveCrmChartsDto
        {
            EnergyConsumption = BuildEnergyConsumptionChart(currentRows, from, to, rangeKey, config),
            PeakNonPeak = BuildPeakNonPeakChart(currentRows, from, to, rangeKey, config),
            HighDemand = BuildHighDemandChart(currentRows, from, to, rangeKey, config),
            UtilityTrend = utilityTrend,
            UtilityMix = utilityMix
        };
    }

    private static CrmDashboardChartResponseDTO BuildEnergyConsumptionChart(
        List<ReadingRow> rows, DateTime from, DateTime to, string rangeKey, ConfigurationContext config)
    {
        var bucket = ResolveCrmBucket(from, to, config.TimeZoneId);
        var totals = BucketEnergy(rows, bucket);
        var keys = BuildBucketKeys(from, to, to - from, config.TimeZoneId).ToList();
        var values = keys.Select(key => Round(totals.GetValueOrDefault(key))).ToList();

        return new CrmDashboardChartResponseDTO
        {
            chartType = "energyconsumption",
            range = rangeKey,
            fromDate = from.ToString("o"),
            toDate = to.ToString("o"),
            unit = "kWh",
            categories = keys.Select(key => FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId)).ToList(),
            series = new List<CrmDashboardChartSeriesDTO>
            {
                new() { name = "Energy Consumption", data = values }
            },
            points = keys.Select((key, index) => new CrmDashboardChartPointDTO
            {
                label = FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId),
                period = key.ToString("o"),
                value = values[index],
                totalKwh = values[index]
            }).ToList(),
            totalKwh = Round(values.Sum())
        };
    }

    private static CrmDashboardChartResponseDTO BuildPeakNonPeakChart(
        List<ReadingRow> rows,
        DateTime from,
        DateTime to,
        string rangeKey,
        ConfigurationContext config)
    {
        if (!config.Features.PeakOffPeakAnalysis)
        {
            return new CrmDashboardChartResponseDTO
            {
                chartType = "peaknonpeak",
                range = rangeKey,
                fromDate = from.ToString("o"),
                toDate = to.ToString("o"),
                unit = "kWh"
            };
        }

        var bucket = ResolveCrmBucket(from, to, config.TimeZoneId);
        var keys = BuildBucketKeys(from, to, to - from, config.TimeZoneId).ToList();
        var totals = keys.ToDictionary(key => key, _ => (peak: 0d, nonPeak: 0d));

        foreach (var interval in CalculateEnergyIntervals(rows))
        {
            var key = bucket(interval.At);
            if (!totals.ContainsKey(key)) totals[key] = (0, 0);
            var current = totals[key];
            if (IsPeak(interval.At, config)) current.peak += interval.EnergyKwh;
            else current.nonPeak += interval.EnergyKwh;
            totals[key] = current;
        }

        var peak = keys.Select(key => Round(totals.GetValueOrDefault(key).peak)).ToList();
        var nonPeak = keys.Select(key => Round(totals.GetValueOrDefault(key).nonPeak)).ToList();
        var peakWindow = config.TariffWindows.FirstOrDefault(x =>
            string.Equals(x.Type, "peak", StringComparison.OrdinalIgnoreCase));

        return new CrmDashboardChartResponseDTO
        {
            chartType = "peaknonpeak",
            range = rangeKey,
            fromDate = from.ToString("o"),
            toDate = to.ToString("o"),
            unit = "kWh",
            categories = keys.Select(key => FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId)).ToList(),
            series = new List<CrmDashboardChartSeriesDTO>
            {
                new() { name = "Peak kWh", data = peak },
                new() { name = "Non-Peak kWh", data = nonPeak }
            },
            points = keys.Select((key, index) => new CrmDashboardChartPointDTO
            {
                label = FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId),
                period = key.ToString("o"),
                peakKwh = peak[index],
                nonPeakKwh = nonPeak[index],
                totalKwh = Round(peak[index] + nonPeak[index]),
                value = Round(peak[index] + nonPeak[index])
            }).ToList(),
            totalPeakKwh = Round(peak.Sum()),
            totalNonPeakKwh = Round(nonPeak.Sum()),
            totalKwh = Round(peak.Sum() + nonPeak.Sum()),
            peakStartTime = peakWindow?.Start.ToString("HH:mm") ?? string.Empty,
            peakEndTime = peakWindow?.End.ToString("HH:mm") ?? string.Empty
        };
    }

    private static CrmDashboardChartResponseDTO BuildHighDemandChart(
        List<ReadingRow> rows,
        DateTime from,
        DateTime to,
        string rangeKey,
        ConfigurationContext config)
    {
        var interval = Math.Clamp(Math.Max(15, config.DemandIntervalMinutes), 15, 60);
        var bucket = ResolveCrmBucket(from, to, config.TimeZoneId);
        var keys = BuildBucketKeys(from, to, to - from, config.TimeZoneId).ToList();
        var intervalDemand = rows
            .GroupBy(x =>
            {
                var minute = x.At.Minute - (x.At.Minute % interval);
                return new DateTime(x.At.Year, x.At.Month, x.At.Day, x.At.Hour, minute, 0, DateTimeKind.Utc);
            })
            .Select(g => new
            {
                At = g.Key,
                DemandW = g.GroupBy(x => x.SensorId).Sum(sensor => sensor.Average(x => x.ActivePower))
            })
            .ToList();
        var demandByBucket = intervalDemand
            .GroupBy(x => bucket(x.At))
            .ToDictionary(g => g.Key, g => g.Max(x => x.DemandW));
        var values = keys.Select(key => Round(demandByBucket.GetValueOrDefault(key))).ToList();

        return new CrmDashboardChartResponseDTO
        {
            chartType = "highdemand",
            range = rangeKey,
            fromDate = from.ToString("o"),
            toDate = to.ToString("o"),
            unit = "W",
            categories = keys.Select(key => FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId)).ToList(),
            series = new List<CrmDashboardChartSeriesDTO>
            {
                new() { name = "Peak Demand", data = values }
            },
            points = keys.Select((key, index) => new CrmDashboardChartPointDTO
            {
                label = FormatCrmBucketLabel(key, rangeKey, config.TimeZoneId),
                period = key.ToString("o"),
                demandW = values[index],
                value = values[index]
            }).ToList(),
            peakDemandW = values.Count > 0 ? Round(values.Max()) : 0
        };
    }

    private static CrmDashboardChartResponseDTO BuildUtilityTrendChart(
        List<ReadingRow> rows, DateTime from, DateTime to, string timeZoneId)
    {
        var monthKeys = BuildMonthKeys(from, to, timeZoneId).TakeLast(12).ToList();
        var utilityNames = rows.Select(x => NormalizedUtilityName(x.UtilityName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        var totals = CalculateEnergyIntervals(rows)
            .GroupBy(x => new
            {
                Month = ResolveLocalBucketStartUtc(x.At, "month", timeZoneId),
                Utility = NormalizedUtilityName(x.UtilityName)
            })
            .ToDictionary(
                g => (g.Key.Month, g.Key.Utility),
                g => Round(g.Sum(x => x.EnergyKwh)));

        var series = utilityNames.Select(utility => new CrmDashboardChartSeriesDTO
        {
            name = utility,
            data = monthKeys
                .Select(month => totals.GetValueOrDefault((month, utility)))
                .ToList()
        }).ToList();

        return new CrmDashboardChartResponseDTO
        {
            chartType = "utilitytrend",
            range = "1y",
            fromDate = from.ToString("o"),
            toDate = to.ToString("o"),
            unit = "kWh",
            categories = monthKeys
                .Select(month => ConvertToConfiguredTime(month, timeZoneId).ToString("MMM yyyy"))
                .ToList(),
            series = series,
            points = monthKeys.Select(month => new CrmDashboardChartPointDTO
            {
                label = ConvertToConfiguredTime(month, timeZoneId).ToString("MMM yyyy"),
                period = month.ToString("o"),
                value = Round(utilityNames.Sum(
                    utility => totals.GetValueOrDefault((month, utility)))),
                totalKwh = Round(utilityNames.Sum(
                    utility => totals.GetValueOrDefault((month, utility))))
            }).ToList(),
            totalKwh = Round(series.Sum(x => x.data.Sum()))
        };
    }

    private static CrmDashboardChartResponseDTO BuildUtilityMixChart(
        List<ReadingRow> rows, DateTime from, DateTime to)
    {
        var totals = CalculateEnergyIntervals(rows)
            .GroupBy(x => NormalizedUtilityName(x.UtilityName), StringComparer.OrdinalIgnoreCase)
            .Select(g => new { Utility = g.Key, Energy = Round(g.Sum(x => x.EnergyKwh)) })
            .OrderByDescending(x => x.Energy)
            .ToList();
        var total = totals.Sum(x => x.Energy);

        return new CrmDashboardChartResponseDTO
        {
            chartType = "utilitymix",
            range = "30d",
            fromDate = from.ToString("o"),
            toDate = to.ToString("o"),
            unit = "kWh",
            categories = totals.Select(x => x.Utility).ToList(),
            series = new List<CrmDashboardChartSeriesDTO>
            {
                new() { name = "Utility Mix", data = totals.Select(x => x.Energy).ToList() }
            },
            points = totals.Select(x => new CrmDashboardChartPointDTO
            {
                label = x.Utility,
                period = to.ToString("o"),
                value = x.Energy,
                totalKwh = x.Energy
            }).ToList(),
            totalKwh = Round(total)
        };
    }

    private static List<EnergyInterval> CalculateEnergyIntervals(List<ReadingRow> rows) =>
        rows.Where(x => x.EnergyKwh > 0)
            .Select(x => new EnergyInterval
            {
                SensorId = x.SensorId,
                At = x.At,
                UtilityName = x.UtilityName,
                EnergyKwh = x.EnergyKwh
            })
            .ToList();

    private static Func<DateTime, DateTime> ResolveCrmBucket(
        DateTime from, DateTime to, string timeZoneId)
    {
        var span = to - from;
        var granularity = span.TotalDays > 2 ? "day" : "hour";
        return value => ResolveLocalBucketStartUtc(value, granularity, timeZoneId);
    }

    private static IEnumerable<DateTime> BuildMonthKeys(
        DateTime from, DateTime to, string timeZoneId)
    {
        var localFrom = ConvertToConfiguredTime(from, timeZoneId);
        var localCursor = new DateTime(
            localFrom.Year,
            localFrom.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Unspecified);

        while (true)
        {
            var utc = ConvertLocalToUtc(localCursor, timeZoneId);
            if (utc >= to)
                yield break;

            yield return utc;
            localCursor = localCursor.AddMonths(1);
        }
    }

    private static string FormatCrmBucketLabel(
        DateTime value, string rangeKey, string timeZoneId)
    {
        var local = ConvertToConfiguredTime(value, timeZoneId);
        return rangeKey switch
        {
            "24h" => local.ToString("HH:mm"),
            "1y" => local.ToString("dd MMM yyyy"),
            _ => local.ToString("dd MMM")
        };
    }

    private static DateTime ResolveLocalBucketStartUtc(
        DateTime value, string granularity, string timeZoneId)
    {
        var local = ConvertToConfiguredTime(value, timeZoneId);
        var localStart = granularity switch
        {
            "month" => new DateTime(local.Year, local.Month, 1, 0, 0, 0, DateTimeKind.Unspecified),
            "day" => new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Unspecified),
            _ => new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0, DateTimeKind.Unspecified)
        };
        return ConvertLocalToUtc(localStart, timeZoneId);
    }

    private static DateTime ConvertLocalToUtc(DateTime local, string timeZoneId)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (string.IsNullOrWhiteSpace(timeZoneId) ||
            timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);

        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (zone.IsInvalidTime(local))
                local = local.AddHours(1);
            if (zone.IsAmbiguousTime(local))
            {
                var offset = zone.GetAmbiguousTimeOffsets(local).Max();
                return new DateTimeOffset(local, offset).UtcDateTime;
            }
            return TimeZoneInfo.ConvertTimeToUtc(local, zone);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);
        }
        catch (InvalidTimeZoneException)
        {
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);
        }
    }

    private static string NormalizedUtilityName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();

    private static List<DeepDiveTrendPointDto> BuildTrend(
        List<ReadingRow> current,
        List<ReadingRow> previous,
        DateTime from,
        DateTime to,
        ConfigurationContext config)
    {
        var span = to - from;
        var granularity = span.TotalDays > 2 ? "day" : "hour";
        Func<DateTime, DateTime> bucket = value =>
            ResolveLocalBucketStartUtc(value, granularity, config.TimeZoneId);

        var currentEnergy = BucketEnergy(current, bucket);
        var previousEnergy = BucketEnergy(previous, bucket);
        var currentGroups = current.GroupBy(x => bucket(x.At))
            .ToDictionary(g => g.Key, g => g.ToList());
        var previousGroups = previous.GroupBy(x => bucket(x.At))
            .ToDictionary(g => g.Key, g => g.ToList());

        var keys = BuildBucketKeys(from, to, span, config.TimeZoneId).ToList();
        var previousKeys = BuildBucketKeys(from - span, from, span, config.TimeZoneId).ToList();

        return keys.Select((key, index) =>
        {
            currentEnergy.TryGetValue(key, out var energy);
            var previousKey = index < previousKeys.Count ? previousKeys[index] : key - span;
            previousEnergy.TryGetValue(previousKey, out var previousBucketEnergy);

            var rows = currentGroups.GetValueOrDefault(key) ?? new List<ReadingRow>();
            var previousRows = previousGroups.GetValueOrDefault(previousKey) ?? new List<ReadingRow>();
            var demand = rows.Count == 0
                ? 0
                : rows.GroupBy(x => x.SensorId).Sum(WeightedAverageActivePower) / 1000.0;
            var previousDemand = previousRows.Count == 0
                ? 0
                : previousRows.GroupBy(x => x.SensorId).Sum(WeightedAverageActivePower) / 1000.0;

            var local = ConvertToConfiguredTime(key, config.TimeZoneId);
            return new DeepDiveTrendPointDto
            {
                Bucket = key,
                Label = granularity == "day"
                    ? local.ToString("dd MMM yyyy")
                    : local.ToString("dd MMM HH:mm"),
                EnergyKwh = Round(energy),
                PreviousEnergyKwh = Round(previousBucketEnergy),
                DemandKw = Round(demand),
                PreviousDemandKw = Round(previousDemand),
                Cost = config.Features.CostAnalysis ? Round(CalculateCost(rows, config)) : null,
                PreviousCost = config.Features.CostAnalysis ? Round(CalculateCost(previousRows, config)) : null
            };
        }).ToList();
    }

    private static IEnumerable<DateTime> BuildBucketKeys(
        DateTime from, DateTime to, TimeSpan span, string timeZoneId)
    {
        var granularity = span.TotalDays > 2 ? "day" : "hour";
        var localFrom = ConvertToConfiguredTime(from, timeZoneId);
        var localCursor = granularity == "day"
            ? new DateTime(localFrom.Year, localFrom.Month, localFrom.Day, 0, 0, 0, DateTimeKind.Unspecified)
            : new DateTime(localFrom.Year, localFrom.Month, localFrom.Day, localFrom.Hour, 0, 0, DateTimeKind.Unspecified);

        while (true)
        {
            var utc = ConvertLocalToUtc(localCursor, timeZoneId);
            if (utc >= to) yield break;
            yield return utc;
            localCursor = granularity == "day"
                ? localCursor.AddDays(1)
                : localCursor.AddHours(1);
        }
    }

    private static Dictionary<DateTime, double> BucketEnergy(
        List<ReadingRow> rows,
        Func<DateTime, DateTime> bucket) =>
        rows.Where(x => x.EnergyKwh > 0)
            .GroupBy(x => bucket(x.At))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.EnergyKwh));

    private static double WeightedAverageActivePower(IEnumerable<ReadingRow> rows)
    {
        double weighted = 0;
        long samples = 0;
        foreach (var row in rows)
        {
            if (row.SampleCount <= 0) continue;
            weighted += row.ActivePower * row.SampleCount;
            samples += row.SampleCount;
        }

        return samples == 0 ? 0 : weighted / samples;
    }

    private static DeepDiveDemandDto BuildDemand(
        List<ReadingRow> rows,
        ConfigurationContext config)
    {
        var interval = Math.Clamp(Math.Max(15, config.DemandIntervalMinutes), 15, 60);
        var buckets = rows.GroupBy(x =>
        {
            var minute = x.At.Minute - (x.At.Minute % interval);
            return new DateTime(x.At.Year, x.At.Month, x.At.Day, x.At.Hour, minute, 0, DateTimeKind.Utc);
        })
        .Select(g => new
        {
            At = g.Key,
            Kw = g.GroupBy(x => x.SensorId).Sum(WeightedAverageActivePower) / 1000.0
        })
        .OrderBy(x => x.At)
        .ToList();

        var peak = buckets.OrderByDescending(x => x.Kw).FirstOrDefault();
        var warningKw = config.DemandLimitKw.HasValue
            ? config.DemandLimitKw.Value * config.WarningThresholdPercent / 100.0
            : (double?)null;

        return new DeepDiveDemandDto
        {
            HasData = buckets.Count > 0,
            HasConfiguredLimit = config.DemandLimitKw.HasValue,
            UnavailableReason = buckets.Count == 0
                ? "No demand readings are available for this period."
                : !config.DemandLimitKw.HasValue
                    ? "Demand limit is not configured; peak demand is shown without breach analysis."
                    : string.Empty,
            IntervalMinutes = interval,
            AverageDemandKw = buckets.Count > 0 ? Round(buckets.Average(x => x.Kw)) : 0,
            PeakDemandKw = peak is null ? 0 : Round(peak.Kw),
            PeakDemandAt = peak?.At,
            DemandLimitKw = config.DemandLimitKw,
            WarningThresholdKw = warningKw.HasValue ? Round(warningKw.Value) : null,
            BreachCount = config.DemandLimitKw.HasValue
                ? buckets.Count(x => x.Kw > config.DemandLimitKw.Value)
                : null,
            MinutesAboveThreshold = config.DemandLimitKw.HasValue
                ? buckets.Count(x => x.Kw > config.DemandLimitKw.Value) * interval
                : null
        };
    }

    private async Task<SensorHealthContext> BuildSensorHealthAsync(
        List<Guid> sensorIds,
        int onlineThresholdSeconds)
    {
        var context = new SensorHealthContext();
        if (sensorIds.Count == 0) return context;

        var sensorProfiles = await _db.tbl_sensor.AsNoTracking()
            .Where(x => sensorIds.Contains(x.sensor_id))
            .Select(x => new
            {
                x.sensor_id,
                x.sensor_name,
                x.device.device_name,
                Appliance = x.sensor_appliances
                    .Where(a => a.is_active && !a.is_deleted &&
                                a.appliance.is_active && !a.appliance.is_deleted)
                    .Select(a => new
                    {
                        a.appliance.appliance_name,
                        a.appliance.max_power,
                        a.appliance.standby_power,
                        a.appliance.normal_power_factor
                    }).FirstOrDefault()
            })
            .ToListAsync();

        var latest = await _energyAggregateStore.LoadLatestAsync(sensorIds);

        var onlineCutoff = DateTime.UtcNow.AddSeconds(-Math.Max(30, onlineThresholdSeconds));
        foreach (var row in latest.Values)
        {
            if (row.At >= onlineCutoff)
                context.OnlineSensorIds.Add(row.SensorId);

            var sensor = sensorProfiles.FirstOrDefault(x => x.sensor_id == row.SensorId);
            if (sensor is null) continue;
            var applianceName = sensor.Appliance?.appliance_name ?? string.Empty;

            if (row.Voltage < 210 || row.Voltage > 250)
                context.Issues.Add(NewIssue(
                    sensor.sensor_id, sensor.sensor_name, sensor.device_name, applianceName,
                    "Voltage", "Critical",
                    $"Voltage is {row.Voltage:F1} V and outside the 210–250 V range.",
                    row.Voltage, row.At));

            var expectedPf = sensor.Appliance?.normal_power_factor > 0
                ? sensor.Appliance.normal_power_factor
                : 0.85f;
            if (row.PowerFactor > 0 && row.PowerFactor < expectedPf)
                context.Issues.Add(NewIssue(
                    sensor.sensor_id, sensor.sensor_name, sensor.device_name, applianceName,
                    "Power factor", "Warning",
                    $"Power factor is {row.PowerFactor:F2}; expected at least {expectedPf:F2}.",
                    row.PowerFactor, row.At));

            if (sensor.Appliance?.max_power > 0 && row.ActivePower > sensor.Appliance.max_power)
                context.Issues.Add(NewIssue(
                    sensor.sensor_id, sensor.sensor_name, sensor.device_name, applianceName,
                    "High power", "Critical",
                    $"Active power is {row.ActivePower:F0} W, above the configured {sensor.Appliance.max_power:F0} W maximum.",
                    row.ActivePower, row.At));

            if (sensor.Appliance?.standby_power > 0 &&
                row.ActivePower > 0 && row.ActivePower <= sensor.Appliance.standby_power)
                context.Issues.Add(NewIssue(
                    sensor.sensor_id, sensor.sensor_name, sensor.device_name, applianceName,
                    "Standby usage", "Advisory",
                    $"Power is within the configured standby range ({sensor.Appliance.standby_power:F0} W).",
                    row.ActivePower, row.At));
        }

        context.Issues = context.Issues
            .OrderBy(x => x.Severity == "Critical" ? 0 : x.Severity == "Warning" ? 1 : 2)
            .ThenByDescending(x => x.DetectedAt)
            .Take(30)
            .ToList();
        return context;
    }

    private static DeepDiveIssueDto NewIssue(
        Guid id,
        string sensor,
        string device,
        string appliance,
        string type,
        string severity,
        string message,
        double value,
        DateTime at) => new()
    {
        SensorId = id,
        SensorName = sensor,
        DeviceName = device,
        ApplianceName = appliance,
        IssueType = type,
        Severity = severity,
        Message = message,
        CurrentValue = value,
        DetectedAt = at
    };

    private async Task<List<DeepDiveChildDto>> BuildChildrenAsync(
        string level,
        Guid id,
        List<ReadingRow> currentRows,
        List<ReadingRow> previousRows,
        double parentEnergy,
        ConfigurationContext config,
        SensorHealthContext health,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        var refs = await LoadChildRefsAsync(level, id, allowedOfficeIds);
        if (refs.Count == 0) return new List<DeepDiveChildDto>();

        var mappings = await LoadChildSensorMappingsAsync(level, id, allowedOfficeIds);
        var sensorIdsByChild = mappings
            .GroupBy(x => x.ChildId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.SensorId).Distinct().ToHashSet());
        var currentBySensor = currentRows.ToLookup(x => x.SensorId);
        var previousBySensor = previousRows.ToLookup(x => x.SensorId);
        var issuesBySensor = health.Issues.ToLookup(x => x.SensorId);

        var result = new List<DeepDiveChildDto>(refs.Count);
        foreach (var child in refs)
        {
            var ids = sensorIdsByChild.GetValueOrDefault(child.id) ?? new HashSet<Guid>();
            if (allowedOfficeIds is not null && ids.Count == 0) continue;
            var rows = ids.SelectMany(sensorId => currentBySensor[sensorId]).ToList();
            var previous = ids.SelectMany(sensorId => previousBySensor[sensorId]).ToList();
            var energy = CalculateEnergy(rows);
            var prevEnergy = CalculateEnergy(previous);
            var demand = BuildDemand(rows, config);
            var issueCount = ids.Sum(sensorId => issuesBySensor[sensorId].Count());
            var online = ids.Count(sensorId => health.OnlineSensorIds.Contains(sensorId));
            var change = ChangePercent(energy, prevEnergy);

            result.Add(new DeepDiveChildDto
            {
                Id = child.id,
                Name = child.name,
                Level = child.level,
                EnergyKwh = Round(energy),
                EstimatedCost = config.Features.CostAnalysis
                    ? Round(CalculateCost(rows, config))
                    : null,
                PreviousEnergyKwh = Round(prevEnergy),
                ChangePercent = change,
                PeakDemandKw = demand.PeakDemandKw,
                SensorCount = ids.Count,
                OnlineSensorCount = online,
                IssueCount = issueCount,
                Status = issueCount > 0 || change > 15
                    ? "High"
                    : change > 5 ? "Review" : "Normal"
            });
        }

        foreach (var item in result)
            item.SharePercent = parentEnergy > 0
                ? Round(item.EnergyKwh * 100 / parentEnergy)
                : 0;

        return result.OrderByDescending(x => x.EnergyKwh).ToList();
    }

    private static double CalculateCost(List<ReadingRow> rows, ConfigurationContext config)
    {
        if (!config.Features.CostAnalysis) return 0;
        return rows.Where(x => x.EnergyKwh > 0)
            .Sum(x => x.EnergyKwh * ResolveRate(x.At, config));
    }

    private async Task<List<(Guid id, string name, string level)>> LoadChildRefsAsync(string level, Guid id, HashSet<Guid>? allowedOfficeIds = null)
    {
        if (level == "business")
            return (await _db.tbl_facility.Where(x => x.fk_business == id && !x.is_deleted)
                .Select(x => new { x.facility_id, x.facility_name }).ToListAsync())
                .Select(x => (x.facility_id, x.facility_name, "facility")).ToList();

        if (level == "facility")
            return (await _db.tbl_building.Where(x => x.fk_facility == id && !x.is_deleted)
                .Select(x => new { x.building_id, x.building_name }).ToListAsync())
                .Select(x => (x.building_id, x.building_name, "building")).ToList();

        if (level == "building")
            return (await _db.tbl_floor.Where(x => x.fk_building == id && !x.is_deleted)
                .Select(x => new { x.floor_id, x.floor_name }).ToListAsync())
                .Select(x => (x.floor_id, x.floor_name, "floor")).ToList();

        if (level == "floor")
            return (await _db.tbl_section.Where(x => x.fk_floor == id && !x.is_deleted)
                .Select(x => new { x.section_id, x.section_name }).ToListAsync())
                .Select(x => (x.section_id, x.section_name, "section")).ToList();

        if (level == "section")
            return (await _db.tbl_office.Where(x => x.fk_section == id && !x.is_deleted)
                .Select(x => new { x.office_id, x.office_name }).ToListAsync())
                .Select(x => (x.office_id, x.office_name, "office")).ToList();

        if (level == "office")
            return (await _db.tbl_device.Where(x => x.fk_office == id && !x.is_deleted)
                .Select(x => new { x.device_id, x.device_name }).ToListAsync())
                .Select(x => (x.device_id, x.device_name, "device")).ToList();

        if (level == "device")
            return (await _db.tbl_sensor.Where(x => x.fk_device == id && !x.is_deleted)
                .Select(x => new { x.sensor_id, x.sensor_name }).ToListAsync())
                .Select(x => (x.sensor_id, x.sensor_name, "sensor")).ToList();

        return new();
    }

    private async Task<List<ChildSensorMap>> LoadChildSensorMappingsAsync(
        string level,
        Guid id,
        HashSet<Guid>? allowedOfficeIds = null)
    {
        IQueryable<ChildSensorMap> query = level switch
        {
            "business" => _db.tbl_sensor
                .Where(s => !s.is_deleted && s.device.fk_business == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)) &&
                            !s.device.is_deleted && !s.device.office.is_deleted)
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.office.section.floor.building.fk_facility,
                    SensorId = s.sensor_id
                }),
            "facility" => _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.section.floor.building.fk_facility == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.office.section.floor.building.building_id,
                    SensorId = s.sensor_id
                }),
            "building" => _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.section.floor.fk_building == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.office.section.floor.floor_id,
                    SensorId = s.sensor_id
                }),
            "floor" => _db.tbl_sensor
                .Where(s => !s.is_deleted && s.device.office.section.fk_floor == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.office.section.section_id,
                    SensorId = s.sensor_id
                }),
            "section" => _db.tbl_sensor
                .Where(s => !s.is_deleted && s.device.office.fk_section == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.office.office_id,
                    SensorId = s.sensor_id
                }),
            "office" => _db.tbl_sensor
                .Where(s => !s.is_deleted && s.device.fk_office == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.device.device_id,
                    SensorId = s.sensor_id
                }),
            "device" => _db.tbl_sensor
                .Where(s => !s.is_deleted && s.fk_device == id &&
                            (allowedOfficeIds == null || allowedOfficeIds.Contains(s.device.fk_office)))
                .Select(s => new ChildSensorMap
                {
                    ChildId = s.sensor_id,
                    SensorId = s.sensor_id
                }),
            _ => _db.tbl_sensor.Where(s => false)
                .Select(s => new ChildSensorMap
                {
                    ChildId = Guid.Empty,
                    SensorId = Guid.Empty
                })
        };

        return await query.AsNoTracking().ToListAsync();
    }

    private async Task<List<DeepDiveSuggestionDto>> LoadSuggestionsAsync(
        Guid businessId,
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        ConfigurationContext config,
        HashSet<Guid>? allowedOfficeIds = null,
        bool includeBusinessLevel = false)
    {
        if (!config.Features.OptimizationSuggestions) return new();

        var nowUtc = DateTime.UtcNow;
        var query = _db.tbl_dashboard_suggestion.AsNoTracking()
            .Where(x => x.fk_business == businessId
                && ((x.reason_code.StartsWith("LIVE_") && x.to_time >= nowUtc)
                    || (!x.reason_code.StartsWith("LIVE_") && x.to_time >= from && x.from_time <= to)));

        var relevantOfficeIds = allowedOfficeIds ?? (sensorIds.Count == 0
            ? new HashSet<Guid>()
            : (await _db.tbl_sensor.AsNoTracking()
                .Where(x => sensorIds.Contains(x.sensor_id))
                .Select(x => x.device.fk_office)
                .Distinct()
                .ToListAsync()).ToHashSet());

        query = query.Where(x =>
            (x.fk_sensor.HasValue && sensorIds.Contains(x.fk_sensor.Value))
            || (x.fk_office.HasValue && relevantOfficeIds.Contains(x.fk_office.Value))
            || (includeBusinessLevel && !x.fk_sensor.HasValue && !x.fk_office.HasValue));

        var rows = await query
            .OrderByDescending(x => x.updated_at)
            .Take(100)
            .ToListAsync();

        var suggestions = rows
            .GroupBy(x => x.reason_code)
            .Select(x => x.OrderByDescending(y => y.updated_at).First())
            .OrderByDescending(x => string.Equals(x.priority, "High", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => x.estimated_saving_kwh)
            .Take(20)
            .Select(x => new DeepDiveSuggestionDto
            {
                Priority = x.priority,
                Title = x.title,
                ReasonCode = x.reason_code,
                Recommendation = string.IsNullOrWhiteSpace(x.recommendation) ? x.action : x.recommendation,
                EstimatedSavingKwh = x.estimated_saving_kwh,
                EstimatedSavingCost = x.estimated_saving_cost,
                CanApplyAction = x.can_apply_action
            }).ToList();

        if (!config.Features.SavingsCostAnalysis)
            foreach (var suggestion in suggestions) suggestion.EstimatedSavingCost = null;

        return suggestions;
    }

    private async Task<List<DeepDiveBreadcrumbDto>> BuildBreadcrumbsAsync(string level, Guid id)
    {
        if (level == "business")
        {
            var business = await _db.tbl_business.Where(x => x.business_id == id && !x.is_deleted)
                .Select(x => new DeepDiveBreadcrumbDto { Level = "business", Id = x.business_id, Name = x.business_name })
                .FirstOrDefaultAsync();
            return business is null ? new() : new() { business };
        }

        if (level == "sensor")
        {
            var row = await _db.tbl_sensor.AsNoTracking()
                .Where(x => x.sensor_id == id && !x.is_deleted)
                .Select(x => new
                {
                    BusinessId = x.device.business.business_id,
                    BusinessName = x.device.business.business_name,
                    FacilityId = x.device.office.section.floor.building.facility.facility_id,
                    FacilityName = x.device.office.section.floor.building.facility.facility_name,
                    BuildingId = x.device.office.section.floor.building.building_id,
                    BuildingName = x.device.office.section.floor.building.building_name,
                    FloorId = x.device.office.section.floor.floor_id,
                    FloorName = x.device.office.section.floor.floor_name,
                    SectionId = x.device.office.section.section_id,
                    SectionName = x.device.office.section.section_name,
                    OfficeId = x.device.office.office_id,
                    OfficeName = x.device.office.office_name,
                    DeviceId = x.device.device_id,
                    DeviceName = x.device.device_name,
                    SensorId = x.sensor_id,
                    SensorName = x.sensor_name
                }).FirstOrDefaultAsync();
            if (row is null) return new();
            return new()
            {
                new() { Level = "business", Id = row.BusinessId, Name = row.BusinessName },
                new() { Level = "facility", Id = row.FacilityId, Name = row.FacilityName },
                new() { Level = "building", Id = row.BuildingId, Name = row.BuildingName },
                new() { Level = "floor", Id = row.FloorId, Name = row.FloorName },
                new() { Level = "section", Id = row.SectionId, Name = row.SectionName },
                new() { Level = "office", Id = row.OfficeId, Name = row.OfficeName },
                new() { Level = "device", Id = row.DeviceId, Name = row.DeviceName },
                new() { Level = "sensor", Id = row.SensorId, Name = row.SensorName }
            };
        }

        if (level == "device")
        {
            var row = await _db.tbl_device.AsNoTracking()
                .Where(x => x.device_id == id && !x.is_deleted)
                .Select(x => new
                {
                    BusinessId = x.business.business_id,
                    BusinessName = x.business.business_name,
                    FacilityId = x.office.section.floor.building.facility.facility_id,
                    FacilityName = x.office.section.floor.building.facility.facility_name,
                    BuildingId = x.office.section.floor.building.building_id,
                    BuildingName = x.office.section.floor.building.building_name,
                    FloorId = x.office.section.floor.floor_id,
                    FloorName = x.office.section.floor.floor_name,
                    SectionId = x.office.section.section_id,
                    SectionName = x.office.section.section_name,
                    OfficeId = x.office.office_id,
                    OfficeName = x.office.office_name,
                    DeviceId = x.device_id,
                    DeviceName = x.device_name
                }).FirstOrDefaultAsync();
            if (row is null) return new();
            return new()
            {
                new() { Level = "business", Id = row.BusinessId, Name = row.BusinessName },
                new() { Level = "facility", Id = row.FacilityId, Name = row.FacilityName },
                new() { Level = "building", Id = row.BuildingId, Name = row.BuildingName },
                new() { Level = "floor", Id = row.FloorId, Name = row.FloorName },
                new() { Level = "section", Id = row.SectionId, Name = row.SectionName },
                new() { Level = "office", Id = row.OfficeId, Name = row.OfficeName },
                new() { Level = "device", Id = row.DeviceId, Name = row.DeviceName }
            };
        }

        var sensorIds = await ResolveSensorIdsAsync(level, id);
        if (sensorIds.Count == 0)
        {
            var entity = await ResolveEntityAsync(level, id);
            var businessId = await ResolveBusinessIdAsync(level, id);
            var crumbs = new List<DeepDiveBreadcrumbDto>();
            if (businessId.HasValue)
            {
                var b = await _db.tbl_business.Where(x => x.business_id == businessId.Value)
                    .Select(x => new DeepDiveBreadcrumbDto { Level = "business", Id = x.business_id, Name = x.business_name })
                    .FirstOrDefaultAsync();
                if (b is not null) crumbs.Add(b);
            }
            if (entity.HasValue && level != "business")
                crumbs.Add(new DeepDiveBreadcrumbDto { Level = level, Id = id, Name = entity.Value.name });
            return crumbs;
        }

        var firstSensor = sensorIds[0];
        var full = await BuildBreadcrumbsAsync("sensor", firstSensor);
        var targetIndex = full.FindIndex(x => x.Level == level && x.Id == id);
        return targetIndex >= 0 ? full.Take(targetIndex + 1).ToList() : full;
    }

    private static string ChildLevel(string level) => level switch
    {
        "business" => "facility",
        "facility" => "building",
        "building" => "floor",
        "floor" => "section",
        "section" => "office",
        "office" => "device",
        "device" => "sensor",
        _ => string.Empty
    };

    private static string BuildInsight(
        List<DeepDiveChildDto> children,
        double total,
        DeepDiveConfigurationStatusDto config)
    {
        var top = children.FirstOrDefault();
        if (top is null || total <= 0)
            return "No sufficient consumption data is available for this selection.";

        var baseInsight = $"{top.Name} consumed the most energy at {top.SharePercent:F1}% of the selected hierarchy total.";
        return config.IsReadyForOptimization
            ? baseInsight
            : $"{baseInsight} Complete the missing configuration items before using every cost and optimization result.";
    }

    private static string BuildDataMessage(
        int sensorCount,
        int sensorsWithReadings,
        int readingCount,
        int resetCount,
        int ignoredSpikeCount)
    {
        if (sensorCount == 0) return "No sensors are assigned to this hierarchy level.";
        if (readingCount == 0) return "No meter readings are available in the selected period.";

        var coverage = sensorsWithReadings < sensorCount
            ? $"Data is available for {sensorsWithReadings} of {sensorCount} sensors in the selected period."
            : "Consumption data is available for all sensors in this scope.";

        if (resetCount > 0 || ignoredSpikeCount > 0)
            coverage += $" Meter protection handled {resetCount} reset(s) and ignored {ignoredSpikeCount} implausible spike(s).";

        return coverage;
    }

    private static double ResolveRate(DateTime at, ConfigurationContext config)
    {
        if (!config.Features.CostAnalysis) return 0;
        if (config.Features.PeakOffPeakAnalysis)
            return IsPeak(at, config) ? config.PeakRate : config.OffPeakRate;
        return config.StandardRate;
    }

    private static DateTime ResolveLocalMonthStartUtc(DateTime to, string timeZoneId)
    {
        var instant = EnsureUtc(to).AddTicks(-1);
        if (string.IsNullOrWhiteSpace(timeZoneId) ||
            timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            return new DateTime(instant.Year, instant.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var local = TimeZoneInfo.ConvertTimeFromUtc(instant, zone);
            var localMonthStart = DateTime.SpecifyKind(
                new DateTime(local.Year, local.Month, 1, 0, 0, 0),
                DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localMonthStart, zone);
        }
        catch (TimeZoneNotFoundException)
        {
            return new DateTime(instant.Year, instant.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        catch (InvalidTimeZoneException)
        {
            return new DateTime(instant.Year, instant.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    private static bool IsPeak(DateTime at, ConfigurationContext config) =>
        IsPeak(at, config.TariffWindows, config.TimeZoneId);

    private static bool IsPeak(
        DateTime at,
        IEnumerable<TariffWindow> windows,
        string timeZoneId)
    {
        var localAt = ConvertToConfiguredTime(at, timeZoneId);
        foreach (var window in windows.Where(x =>
                     x.Type.Equals("Peak", StringComparison.OrdinalIgnoreCase)))
        {
            if (window.DayOfWeek.HasValue &&
                window.DayOfWeek.Value != (int)localAt.DayOfWeek)
                continue;

            if (window.SeasonStart.HasValue && window.SeasonEnd.HasValue)
            {
                var date = DateOnly.FromDateTime(localAt);
                if (!InSeason(date, window.SeasonStart.Value, window.SeasonEnd.Value))
                    continue;
            }

            var time = TimeOnly.FromDateTime(localAt);
            if (window.Start <= window.End)
            {
                if (time >= window.Start && time < window.End) return true;
            }
            else if (time >= window.Start || time < window.End)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) return false;
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }

    private static DateTime ConvertToConfiguredTime(DateTime at, string timeZoneId)
    {
        var utc = EnsureUtc(at);
        if (string.IsNullOrWhiteSpace(timeZoneId) ||
            timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            return utc;

        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }
        catch (TimeZoneNotFoundException)
        {
            return utc;
        }
        catch (InvalidTimeZoneException)
        {
            return utc;
        }
    }

    private static bool InSeason(DateOnly value, DateOnly start, DateOnly end)
    {
        if (start <= end) return value >= start && value <= end;
        return value >= start || value <= end;
    }

    private static double ChangePercent(double current, double previous) =>
        previous <= 0 ? (current > 0 ? 100 : 0) : Round((current - previous) * 100 / previous);

    private static double Round(double value) => Math.Round(value, 2);

    private sealed class SensorHealthContext
    {
        public List<DeepDiveIssueDto> Issues { get; set; } = new();
        public HashSet<Guid> OnlineSensorIds { get; } = new();
    }

    private sealed class ChildSensorMap
    {
        public Guid ChildId { get; set; }
        public Guid SensorId { get; set; }
    }

    private sealed class EnergyInterval
    {
        public Guid SensorId { get; set; }
        public DateTime At { get; set; }
        public string UtilityName { get; set; } = "Unknown";
        public double EnergyKwh { get; set; }
    }

    private sealed class ReadingRow
    {
        public Guid SensorId { get; set; }
        public string UtilityName { get; set; } = "Unknown";
        public DateTime At { get; set; }
        public double EnergyKwh { get; set; }
        public double ActivePower { get; set; }
        public double MaxActivePower { get; set; }
        public double Voltage { get; set; }
        public double PowerFactor { get; set; }
        public int SampleCount { get; set; }
        public DateTime FirstReadingAt { get; set; }
        public DateTime LastReadingAt { get; set; }
        public int ResetCount { get; set; }
        public int IgnoredSpikeCount { get; set; }
    }

    private sealed class TariffWindow
    {
        public string Type { get; set; } = string.Empty;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public int? DayOfWeek { get; set; }
        public DateOnly? SeasonStart { get; set; }
        public DateOnly? SeasonEnd { get; set; }
    }

    private sealed class ApplianceProfileRow
    {
        public Guid SensorId { get; set; }
        public double MaxPower { get; set; }
        public double StandbyPower { get; set; }
        public double NormalPowerFactor { get; set; }
    }

    private sealed class OfficeScheduleRow
    {
        public bool Is24Hours { get; init; }
        public string WorkingDays { get; init; } = string.Empty;
        public TimeOnly OpeningTime { get; init; }
        public TimeOnly ClosingTime { get; init; }
    }

    private sealed class ConfigurationContext
    {
        public string Currency { get; set; } = "PKR";
        public string TimeZoneId { get; set; } = "UTC";
        public double StandardRate { get; set; }
        public double PeakRate { get; set; }
        public double OffPeakRate { get; set; }
        public List<TariffWindow> TariffWindows { get; set; } = new();
        public int DemandIntervalMinutes { get; set; } = 15;
        public double? DemandLimitKw { get; set; }
        public double WarningThresholdPercent { get; set; } = 90;
        public int OnlineThresholdSeconds { get; set; } = 120;
        public DeepDiveConfigurationStatusDto Status { get; set; } = new();
        public DeepDiveFeatureAvailabilityDto Features { get; set; } = new();
    }
}
