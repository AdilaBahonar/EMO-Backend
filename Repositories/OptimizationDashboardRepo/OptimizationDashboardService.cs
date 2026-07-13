using EMO.Models.DBModels;
using EMO.Models.DTOs.OptimizationDashboardDTOs;
using EMO.Models.DTOs.RedisRuntimeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorChainRedisDTOs;
using EMO.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace EMO.Repositories.OptimizationDashboardRepo
{
    public class OptimizationDashboardService : IOptimizationDashboardService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redis;
        private readonly RedisKeys redisKeys;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OptimizationDashboardService(
            DBUserManagementContext db,
            IConnectionMultiplexer redis,
            IOptions<RedisKeys> redisKeysOptions)
        {
            this.db = db;
            this.redis = redis.GetDatabase();
            redisKeys = redisKeysOptions.Value;
        }

        public async Task<ResponseModel<OptimizationDashboardResponseDTO>> GetOptimizationDashboardAsync(string level, Guid id, OptimizationQueryParams q, bool includeBusinessSuggestions = true)
        {
            try
            {
                var normalizedLevel = (level ?? string.Empty).Trim().ToLowerInvariant();
                var resolvedRange = ResolveRange(q);
                var from = resolvedRange.FromUtc;
                var to = resolvedRange.ToUtc;

                var sensorIds = await GetSensorIdsAsync(normalizedLevel, id);

                if (!sensorIds.Any())
                {
                    return new ResponseModel<OptimizationDashboardResponseDTO>
                    {
                        success = true,
                        remarks = "No sensors found for this level.",
                        data = CreateEmptyResponse(normalizedLevel, id, resolvedRange)
                    };
                }

                var chains = await GetChainsAsync(sensorIds);
                var latestReadings = await GetLatestReadingsAsync(sensorIds);
                var stableReadings = await GetStableReadingsAsync(sensorIds);
                var businessId = chains.Values.Select(x => x.BusinessId).FirstOrDefault(x => x != Guid.Empty);
                var applianceMetadata = await GetApplianceOptimizationMetadataAsync(sensorIds);

                var rows = await db.tbl_singal_phase_data
                    .AsNoTracking()
                    .Where(x => !x.is_deleted && sensorIds.Contains(x.fk_sensor)
                             && x.created_at >= from && x.created_at <= to)
                    .Select(x => new OptimizationReadingRow
                    {
                        SensorId = x.fk_sensor,
                        CreatedAt = x.created_at,
                        ActiveEnergy = (double)x.active_energy,
                        ActivePower = (double)x.active_power,
                        PowerFactor = (double)x.power_factor,
                        Voltage = (double)x.volt,
                        Current = (double)x.current
                    })
                    .ToListAsync();

                var energyResult = EnergyConsumptionCalculator.CalculateSensorWiseConsumption(
                    rows,
                    x => x.SensorId,
                    x => x.CreatedAt,
                    x => x.ActiveEnergy);

                var sensorEnergy = rows
                    .GroupBy(x => x.SensorId)
                    .ToDictionary(
                        g => g.Key,
                        g => EnergyConsumptionCalculator.CalculateSingleSensorConsumption(
                            g,
                            x => x.CreatedAt,
                            x => x.ActiveEnergy).Consumption);

                var livePower = latestReadings.Values.Sum(x => x.ActivePower);
                var peakPower = rows.Any() ? rows.Max(x => x.ActivePower) : 0;

                var idleAppliances = BuildIdleAppliances(chains, latestReadings, stableReadings);
                var faultyAppliances = BuildFaultyAppliances(chains, latestReadings, stableReadings, applianceMetadata);
                var highConsumers = BuildHighConsumers(chains, latestReadings, sensorEnergy, energyResult.Consumption);
                var utilityBreakdown = BuildUtilityBreakdown(chains, latestReadings, sensorEnergy, energyResult.Consumption);
                var comparisons = BuildComparisons(normalizedLevel, chains, latestReadings, sensorEnergy);
                var peakDemandHours = BuildPeakDemandHours(rows);
                var peakDemandSummary = BuildPeakDemandSummary(peakDemandHours);

                var response = new OptimizationDashboardResponseDTO
                {
                    Level = normalizedLevel,
                    EntityId = id.ToString(),
                    Range = resolvedRange.Range,
                    IsCustomRange = resolvedRange.IsCustomRange,
                    FromUtc = resolvedRange.FromUtc,
                    ToUtc = resolvedRange.ToUtc,
                    RangeLabel = resolvedRange.RangeLabel,
                    SensorCount = sensorIds.Count,
                    ActiveSensorCount = latestReadings.Count,
                    TotalEnergyKwh = Math.Round(energyResult.Consumption, 2),
                    CurrentLivePowerW = Math.Round(livePower, 1),
                    PeakPowerW = Math.Round(peakPower, 1),
                    MeterResetCount = energyResult.ResetCount,
                    IgnoredSpikeCount = energyResult.IgnoredSpikeCount,
                    HighConsumers = highConsumers,
                    UtilityBreakdown = utilityBreakdown,
                    IdleAppliances = idleAppliances,
                    FaultyAppliances = faultyAppliances,
                    Comparisons = comparisons,
                    ComparisonChart = comparisons,
                    PeakDemandHours = peakDemandHours,
                    PeakDemandSummary = peakDemandSummary,
                    LiveSensors = BuildLiveSensors(chains, latestReadings, idleAppliances, faultyAppliances)
                };

                // API alerts are an initial snapshot/fallback. The CRM card should use WebSocket live-alerts after connection.
                response.Alerts = BuildInitialAlertsSnapshot(response);
                // Suggestions are created continuously by the Optimization Worker and stored in
                // tbl_dashboard_suggestion. This API only reads the active worker output.
                response.Suggestions = await ReadLiveSuggestionsAsync(businessId, sensorIds, includeBusinessSuggestions);

                return new ResponseModel<OptimizationDashboardResponseDTO>
                {
                    data = response,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<OptimizationDashboardResponseDTO>
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }

        private static OptimizationDashboardResponseDTO CreateEmptyResponse(string normalizedLevel, Guid id, ResolvedOptimizationRange range)
        {
            return new OptimizationDashboardResponseDTO
            {
                Level = normalizedLevel,
                EntityId = id.ToString(),
                Range = range.Range,
                IsCustomRange = range.IsCustomRange,
                FromUtc = range.FromUtc,
                ToUtc = range.ToUtc,
                RangeLabel = range.RangeLabel
            };
        }

        private static ResolvedOptimizationRange ResolveRange(OptimizationQueryParams q)
        {
            q ??= new OptimizationQueryParams();

            var hasCustomRange = q.From.HasValue || q.To.HasValue ||
                                 string.Equals(q.Range, "custom", StringComparison.OrdinalIgnoreCase);

            var toUtc = q.To?.UtcDateTime ?? DateTime.UtcNow;

            var fromUtc = q.From?.UtcDateTime ?? ((q.Range ?? "24h").Trim().ToLowerInvariant() switch
            {
                "7d" => toUtc.AddDays(-7),
                "30d" => toUtc.AddDays(-30),
                "90d" => toUtc.AddDays(-90),
                "1y" => toUtc.AddYears(-1),
                "12m" => toUtc.AddYears(-1),
                "365d" => toUtc.AddDays(-365),
                "year" => toUtc.AddYears(-1),
                "custom" => toUtc.AddHours(-24),
                _ => toUtc.AddHours(-24)
            });

            if (fromUtc.Kind != DateTimeKind.Utc)
                fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);

            if (toUtc.Kind != DateTimeKind.Utc)
                toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

            if (fromUtc > toUtc)
                throw new ArgumentException("From date cannot be greater than To date.");

            if ((toUtc - fromUtc).TotalDays > 366)
                throw new ArgumentException("Custom range cannot be greater than 1 year.");

            var range = hasCustomRange
                ? "custom"
                : (string.IsNullOrWhiteSpace(q.Range) ? "24h" : q.Range.Trim().ToLowerInvariant());

            return new ResolvedOptimizationRange
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Range = range,
                IsCustomRange = hasCustomRange,
                RangeLabel = hasCustomRange
                    ? $"{fromUtc:yyyy-MM-dd HH:mm} UTC - {toUtc:yyyy-MM-dd HH:mm} UTC"
                    : range
            };
        }

        private async Task<List<Guid>> GetSensorIdsAsync(string level, Guid id)
        {
            var redisSetKey = level switch
            {
                "business" => $"{redisKeys.BusinessSensorsKeyPrefix}{id}",
                "facility" => $"{redisKeys.FacilitySensorsKeyPrefix}{id}",
                "building" => $"{redisKeys.BuildingSensorsKeyPrefix}{id}",
                "floor" => $"{redisKeys.FloorSensorsKeyPrefix}{id}",
                "section" => $"{redisKeys.SectionSensorsKeyPrefix}{id}",
                "office" => $"{redisKeys.OfficeSensorsKeyPrefix}{id}",
                "device" => $"{redisKeys.DeviceSensorsKeyPrefix}{id}",
                "sensor" => string.Empty,
                _ => string.Empty
            };

            if (level == "sensor") return new List<Guid> { id };

            if (!string.IsNullOrEmpty(redisSetKey))
            {
                var values = await redis.SetMembersAsync(redisSetKey);
                var ids = values
                    .Select(x => Guid.TryParse(x, out var guid) ? guid : Guid.Empty)
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (ids.Any()) return ids;
            }

            return level switch
            {
                "business" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.fk_business == id).Select(s => s.sensor_id).ToListAsync(),
                "facility" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.floor.building.fk_facility == id).Select(s => s.sensor_id).ToListAsync(),
                "building" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.floor.fk_building == id).Select(s => s.sensor_id).ToListAsync(),
                "floor" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.section.fk_floor == id).Select(s => s.sensor_id).ToListAsync(),
                "section" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.office.fk_section == id).Select(s => s.sensor_id).ToListAsync(),
                "office" => await db.tbl_sensor.Where(s => !s.is_deleted && s.device.fk_office == id).Select(s => s.sensor_id).ToListAsync(),
                "device" => await db.tbl_sensor.Where(s => !s.is_deleted && s.fk_device == id).Select(s => s.sensor_id).ToListAsync(),
                _ => new List<Guid>()
            };
        }

        private async Task<Dictionary<Guid, SensorChainRedisDTO>> GetChainsAsync(List<Guid> sensorIds)
        {
            var result = new Dictionary<Guid, SensorChainRedisDTO>();

            foreach (var sensorId in sensorIds)
            {
                var json = await redis.StringGetAsync($"{redisKeys.SensorChainKeyPrefix}{sensorId}");
                if (!json.HasValue) continue;

                var dto = JsonSerializer.Deserialize<SensorChainRedisDTO>(json!, jsonOptions);
                if (dto != null) result[sensorId] = dto;
            }

            return result;
        }

        private async Task<Dictionary<Guid, SensorLatestRedisDTO>> GetLatestReadingsAsync(List<Guid> sensorIds)
        {
            var result = new Dictionary<Guid, SensorLatestRedisDTO>();

            foreach (var sensorId in sensorIds)
            {
                var json = await redis.StringGetAsync($"sensor:latest:{sensorId}");
                if (!json.HasValue) continue;

                var dto = JsonSerializer.Deserialize<SensorLatestRedisDTO>(json!, jsonOptions);
                if (dto != null) result[sensorId] = dto;
            }

            return result;
        }

        private async Task<Dictionary<Guid, SensorStableRedisDTO>> GetStableReadingsAsync(List<Guid> sensorIds)
        {
            const int maxAgeSeconds = 90;
            var now = DateTime.UtcNow;
            var result = new Dictionary<Guid, SensorStableRedisDTO>();

            foreach (var sensorId in sensorIds)
            {
                var json = await redis.StringGetAsync($"sensor:stable:{sensorId}");
                if (!json.HasValue) continue;

                var dto = JsonSerializer.Deserialize<SensorStableRedisDTO>(json!, jsonOptions);
                if (dto == null || !dto.IsReady || dto.GeneratedAtUtc == default) continue;

                var generatedAtUtc = dto.GeneratedAtUtc.Kind == DateTimeKind.Utc
                    ? dto.GeneratedAtUtc
                    : DateTime.SpecifyKind(dto.GeneratedAtUtc, DateTimeKind.Utc);

                if ((now - generatedAtUtc).TotalSeconds > maxAgeSeconds) continue;
                result[sensorId] = dto;
            }

            return result;
        }

        private async Task<List<OptimizationSuggestionDTO>> ReadLiveSuggestionsAsync(
            Guid businessId,
            List<Guid> sensorIds,
            bool includeBusinessSuggestions)
        {
            if (businessId == Guid.Empty) return new List<OptimizationSuggestionDTO>();

            var now = DateTime.UtcNow;
            var query = db.tbl_dashboard_suggestion
                .AsNoTracking()
                .Where(x => x.fk_business == businessId
                    && x.reason_code.StartsWith("LIVE_")
                    && x.to_time > now);

            query = includeBusinessSuggestions
                ? query.Where(x => x.fk_sensor == null || sensorIds.Contains(x.fk_sensor.Value))
                : query.Where(x => x.fk_sensor.HasValue && sensorIds.Contains(x.fk_sensor.Value));

            var rows = await query
                .OrderByDescending(x => x.updated_at)
                .Take(100)
                .ToListAsync();

            static int SeverityRank(string? severity) => (severity ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "critical" => 4,
                "warning" => 3,
                "opportunity" => 2,
                "info" => 1,
                _ => 0
            };

            return rows
                .GroupBy(x => x.reason_code)
                .Select(x => x.OrderByDescending(y => y.updated_at).First())
                .OrderByDescending(x => SeverityRank(x.severity))
                .ThenByDescending(x => x.updated_at)
                .Take(30)
                .Select(x => new OptimizationSuggestionDTO
                {
                    Severity = x.severity,
                    Priority = x.priority,
                    Type = x.type,
                    ReasonCode = x.reason_code,
                    Title = x.title,
                    Message = x.description,
                    Action = string.IsNullOrWhiteSpace(x.action) ? x.recommendation : x.action,
                    SensorId = x.fk_sensor?.ToString() ?? string.Empty,
                    ApplianceName = x.affected_appliance,
                    UtilityName = x.affected_utility,
                    OfficeName = x.affected_office,
                    TimeBucket = x.updated_at.ToString("O"),
                    EstimatedSavingKwh = x.estimated_saving_kwh ?? 0,
                    EstimatedSavingCost = x.estimated_saving_cost ?? 0,
                    CanApplyAction = x.can_apply_action,
                    ConflictsWithPeakHour = x.conflicts_with_peak_hour
                })
                .ToList();
        }

        private static List<IdleApplianceDTO> BuildIdleAppliances(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, SensorStableRedisDTO> stable)
        {
            var result = new List<IdleApplianceDTO>();

            foreach (var item in stable)
            {
                if (!chains.TryGetValue(item.Key, out var chain)) continue;
                if (!latest.TryGetValue(item.Key, out var r)) continue;
                var evidence = item.Value;
                var currentPower = Math.Max(0, evidence.AveragePowerW);
                var standby = Math.Max(0, chain.StandbyPower);
                if (standby <= 0 || currentPower <= 0) continue;

                var flexibleLimit = Math.Max(standby * 1.5, standby + 8);
                var relayLooksOn = string.Equals(r.RelayState, "ON", StringComparison.OrdinalIgnoreCase) || r.RelayEnabled;

                if (relayLooksOn && currentPower <= flexibleLimit)
                {
                    result.Add(new IdleApplianceDTO
                    {
                        SensorId = item.Key.ToString(),
                        SensorName = chain.SensorName,
                        ApplianceName = string.IsNullOrWhiteSpace(chain.ApplianceName) ? chain.SensorName : chain.ApplianceName,
                        UtilityName = chain.UtilityName,
                        CurrentPowerW = Math.Round(currentPower, 1),
                        StandbyPowerW = Math.Round(standby, 1),
                        FlexibleStandbyLimitW = Math.Round(flexibleLimit, 1),
                        StandbyAutoOff = chain.StandbyAutoOff,
                        CanTurnOff = chain.StandbyAutoOff,
                        ActionLabel = chain.StandbyAutoOff ? "Turn off relay" : "Review standby usage",
                        DeviceName = chain.DeviceName,
                        OfficeName = chain.OfficeName,
                        FloorName = chain.FloorName,
                        BuildingName = chain.BuildingName,
                        FacilityName = chain.FacilityName,
                        MacAddress = chain.MacAddress,
                        SerialAddress = chain.SerialAddress
                    });
                }
            }

            return result.OrderByDescending(x => x.CanTurnOff).ThenByDescending(x => x.CurrentPowerW).ToList();
        }

        private static List<FaultyApplianceDTO> BuildFaultyAppliances(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, SensorStableRedisDTO> stable,
            Dictionary<Guid, ApplianceOptimizationMetadata> applianceMetadata)
        {
            var result = new List<FaultyApplianceDTO>();

            foreach (var item in stable)
            {
                if (!chains.TryGetValue(item.Key, out var chain)) continue;
                if (!latest.TryGetValue(item.Key, out var r)) continue;
                var evidence = item.Value;
                var currentPower = Math.Max(0, evidence.AveragePowerW);
                var maxPower = Math.Max(0, chain.MaxPower);
                var expectedPowerFactor = Math.Max(0, chain.NormalPowerFactor);
                var ratedVoltage = Math.Max(0, chain.RatedVoltage);
                applianceMetadata.TryGetValue(item.Key, out var meta);

                if (maxPower > 0 && currentPower > maxPower * 1.15)
                {
                    result.Add(CreateFaultyAppliance(
                        item.Key,
                        chain,
                        evidence,
                        currentPower,
                        maxPower,
                        $"Power is more than 15% above appliance max rating. Current {currentPower:F0}W, expected max {maxPower:F0}W.",
                        "Inspect appliance, wiring, relay, or sensor calibration."));
                }

                if (currentPower >= 80 && expectedPowerFactor >= 0.60
                    && evidence.AveragePowerFactor > 0
                    && evidence.AveragePowerFactor < expectedPowerFactor * 0.75)
                {
                    result.Add(CreateFaultyAppliance(
                        item.Key,
                        chain,
                        evidence,
                        currentPower,
                        maxPower,
                        $"Average power factor over the confirmed one-minute window is {evidence.AveragePowerFactor:F2}, expected around {expectedPowerFactor:F2}.",
                        "Check appliance condition, loose wiring, capacitor/motor issue, or sensor calibration."));
                }

                if (ratedVoltage > 0 && currentPower >= 50
                    && (evidence.AverageVoltageV < ratedVoltage * 0.85 || evidence.AverageVoltageV > ratedVoltage * 1.15))
                {
                    result.Add(CreateFaultyAppliance(
                        item.Key,
                        chain,
                        evidence,
                        currentPower,
                        maxPower,
                        $"Average voltage over the confirmed one-minute window is outside normal range. Current {evidence.AverageVoltageV:F0}V, rated around {ratedVoltage:F0}V.",
                        "Inspect power supply quality, phase connection, or voltage calibration."));
                }

                var relayLooksOn = string.Equals(r.RelayState, "ON", StringComparison.OrdinalIgnoreCase) || r.RelayEnabled;
                var relayLooksOff = string.Equals(r.RelayState, "OFF", StringComparison.OrdinalIgnoreCase);

                if (relayLooksOn && currentPower <= 1 && chain.MinPower > 20 && meta?.IsCritical != true)
                {
                    result.Add(CreateFaultyAppliance(
                        item.Key,
                        chain,
                        evidence,
                        currentPower,
                        maxPower,
                        "Relay is ON but appliance power is almost 0W.",
                        "Check whether the appliance is disconnected, switched off physically, or relay/sensor wiring is incorrect."));
                }

                if (relayLooksOff && currentPower > Math.Max(25, chain.StandbyPower * 2))
                {
                    result.Add(CreateFaultyAppliance(
                        item.Key,
                        chain,
                        evidence,
                        currentPower,
                        maxPower,
                        $"Relay appears OFF but appliance is still consuming {currentPower:F0}W.",
                        "Check relay wiring, bypass connection, or sensor mapping."));
                }
            }

            return result
                .GroupBy(x => new { x.SensorId, x.Reason })
                .Select(g => g.First())
                .OrderByDescending(x => x.CurrentPowerW)
                .Take(20)
                .ToList();
        }

        private static FaultyApplianceDTO CreateFaultyAppliance(
            Guid sensorId,
            SensorChainRedisDTO chain,
            SensorStableRedisDTO reading,
            double currentPower,
            double expectedMaxPower,
            string reason,
            string action)
        {
            return new FaultyApplianceDTO
            {
                SensorId = sensorId.ToString(),
                SensorName = chain.SensorName,
                ApplianceName = string.IsNullOrWhiteSpace(chain.ApplianceName) ? chain.SensorName : chain.ApplianceName,
                UtilityName = chain.UtilityName,
                CurrentPowerW = Math.Round(currentPower, 1),
                ExpectedMaxPowerW = Math.Round(expectedMaxPower, 1),
                PowerFactor = Math.Round(reading.AveragePowerFactor, 2),
                Reason = reason,
                RecommendedAction = action,
                DeviceName = chain.DeviceName,
                OfficeName = chain.OfficeName,
                FloorName = chain.FloorName,
                BuildingName = chain.BuildingName,
                FacilityName = chain.FacilityName
            };
        }

        private static List<HighConsumerDTO> BuildHighConsumers(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, double> energy,
            double totalEnergy)
        {
            return energy
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .Take(10)
                .Select(x =>
                {
                    chains.TryGetValue(x.Key, out var chain);
                    latest.TryGetValue(x.Key, out var live);

                    return new HighConsumerDTO
                    {
                        SensorId = x.Key.ToString(),
                        SensorName = chain?.SensorName ?? string.Empty,
                        UtilityName = chain?.UtilityName ?? string.Empty,
                        ApplianceName = chain?.ApplianceName ?? string.Empty,
                        EnergyKwh = Math.Round(x.Value, 2),
                        CurrentPowerW = Math.Round(live?.ActivePower ?? 0, 1),
                        SharePercent = totalEnergy > 0 ? Math.Round((x.Value / totalEnergy) * 100, 1) : 0
                    };
                })
                .ToList();
        }

        private static List<UtilityConsumptionDTO> BuildUtilityBreakdown(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, double> energy,
            double totalEnergy)
        {
            return chains
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Value.UtilityName) ? "Unknown" : x.Value.UtilityName)
                .Select(g =>
                {
                    var sensorIds = g.Select(x => x.Key).ToList();
                    var kwh = sensorIds.Sum(id => energy.TryGetValue(id, out var value) ? value : 0);
                    var power = sensorIds.Sum(id => latest.TryGetValue(id, out var value) ? value.ActivePower : 0);

                    return new UtilityConsumptionDTO
                    {
                        UtilityName = g.Key,
                        EnergyKwh = Math.Round(kwh, 2),
                        CurrentPowerW = Math.Round(power, 1),
                        SharePercent = totalEnergy > 0 ? Math.Round((kwh / totalEnergy) * 100, 1) : 0
                    };
                })
                .OrderByDescending(x => x.EnergyKwh)
                .ToList();
        }

        private static List<PeakDemandHourDTO> BuildPeakDemandHours(List<OptimizationReadingRow> rows)
        {
            if (!rows.Any()) return new List<PeakDemandHourDTO>();

            var energyByHour = rows
                .GroupBy(x => x.SensorId)
                .SelectMany(sensorGroup =>
                {
                    var ordered = sensorGroup.OrderBy(x => x.CreatedAt).ToList();
                    var deltas = new List<(DateTime HourUtc, double Delta)>();
                    for (var i = 1; i < ordered.Count; i++)
                    {
                        var previous = ordered[i - 1].ActiveEnergy;
                        var current = ordered[i].ActiveEnergy;
                        if (double.IsNaN(previous) || double.IsNaN(current) || current < 0) continue;
                        var delta = current >= previous ? current - previous : current;
                        if (delta <= 0 || delta > 1000) continue;
                        var hour = TruncateToHourUtc(ordered[i].CreatedAt);
                        deltas.Add((hour, delta));
                    }
                    return deltas;
                })
                .GroupBy(x => x.HourUtc)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Delta));

            var hours = rows
                .GroupBy(x => TruncateToHourUtc(x.CreatedAt))
                .Select(g => new PeakDemandHourDTO
                {
                    HourUtc = g.Key,
                    HourLabel = g.Key.ToString("MMM dd HH:00"),
                    HourOfDay = g.Key.Hour,
                    EnergyKwh = Math.Round(energyByHour.TryGetValue(g.Key, out var kwh) ? kwh : 0, 2),
                    AveragePowerW = Math.Round(g.Average(x => x.ActivePower), 1),
                    PeakPowerW = Math.Round(g.Max(x => x.ActivePower), 1),
                    SensorCount = g.Select(x => x.SensorId).Distinct().Count()
                })
                .OrderBy(x => x.HourUtc)
                .ToList();

            var peakKeys = hours
                .OrderByDescending(x => x.PeakPowerW)
                .ThenByDescending(x => x.EnergyKwh)
                .Take(3)
                .Select(x => x.HourUtc)
                .ToHashSet();

            foreach (var hour in hours)
                hour.IsPeakHour = peakKeys.Contains(hour.HourUtc);

            return hours;
        }

        private static PeakDemandSummaryDTO BuildPeakDemandSummary(List<PeakDemandHourDTO> hours)
        {
            if (!hours.Any()) return new PeakDemandSummaryDTO();

            var peak = hours.OrderByDescending(x => x.PeakPowerW).First();
            var topHoursOfDay = hours
                .Where(x => x.IsPeakHour)
                .GroupBy(x => x.HourOfDay)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Max(x => x.PeakPowerW))
                .Take(3)
                .Select(g => $"{g.Key:00}:00")
                .ToList();

            return new PeakDemandSummaryDTO
            {
                PeakHourLabel = peak.HourLabel,
                PeakPowerW = peak.PeakPowerW,
                PeakEnergyKwh = peak.EnergyKwh,
                RecommendedAvoidHours = string.Join(", ", topHoursOfDay),
                Message = topHoursOfDay.Any()
                    ? $"Peak demand appears around {string.Join(", ", topHoursOfDay)}. Shift non-essential loads outside these hours."
                    : "No strong peak-hour pattern detected."
            };
        }

        private static List<LiveSensorOptimizationDTO> BuildLiveSensors(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            List<IdleApplianceDTO> idle,
            List<FaultyApplianceDTO> faulty)
        {
            var idleSet = idle.Select(x => x.SensorId).ToHashSet();
            var faultySet = faulty.Select(x => x.SensorId).ToHashSet();

            return latest.Select(item =>
            {
                chains.TryGetValue(item.Key, out var chain);
                var live = item.Value;
                return new LiveSensorOptimizationDTO
                {
                    SensorId = item.Key.ToString(),
                    SensorName = chain?.SensorName ?? string.Empty,
                    ApplianceName = chain?.ApplianceName ?? string.Empty,
                    UtilityName = chain?.UtilityName ?? string.Empty,
                    ActivePowerW = Math.Round(live.ActivePower, 1),
                    Voltage = Math.Round(live.Voltage, 1),
                    Current = Math.Round(live.Current, 2),
                    PowerFactor = Math.Round(live.PowerFactor, 2),
                    Frequency = Math.Round(live.Frequency, 1),
                    RelayState = live.RelayState,
                    RelayEnabled = live.RelayEnabled,
                    HvacLoopEnabled = chain?.HvacLoopEnabled ?? false,
                    HvacLoopOnSeconds = chain?.HvacLoopOnSeconds ?? 0,
                    HvacLoopOffSeconds = chain?.HvacLoopOffSeconds ?? 0,
                    ReceivedAtUtc = live.ReceivedAtUtc,
                    IsIdle = idleSet.Contains(item.Key.ToString()),
                    IsFaulty = faultySet.Contains(item.Key.ToString())
                };
            }).OrderByDescending(x => x.ActivePowerW).ToList();
        }

        private static List<OptimizationAlertDTO> BuildInitialAlertsSnapshot(OptimizationDashboardResponseDTO response)
        {
            var alerts = new List<OptimizationAlertDTO>();

            foreach (var faulty in response.FaultyAppliances.Take(10))
            {
                alerts.Add(new OptimizationAlertDTO
                {
                    Severity = "critical",
                    Type = "faulty",
                    Title = "Possible faulty appliance",
                    SensorId = faulty.SensorId,
                    SensorName = faulty.SensorName,
                    ApplianceName = faulty.ApplianceName,
                    Message = faulty.Reason,
                    Action = faulty.RecommendedAction,
                    TimestampUtc = DateTime.UtcNow
                });
            }

            foreach (var idle in response.IdleAppliances.Take(10))
            {
                alerts.Add(new OptimizationAlertDTO
                {
                    Severity = idle.CanTurnOff ? "warning" : "info",
                    Type = "idle",
                    Title = "Idle appliance detected",
                    SensorId = idle.SensorId,
                    SensorName = idle.SensorName,
                    ApplianceName = idle.ApplianceName,
                    Message = $"{idle.ApplianceName} is consuming {idle.CurrentPowerW:F0}W, close to standby threshold {idle.StandbyPowerW:F0}W, while relay is ON.",
                    Action = idle.CanTurnOff ? "Turn off relay" : "Review standby usage",
                    TimestampUtc = DateTime.UtcNow
                });
            }

            return alerts
                .OrderBy(x => x.Severity == "critical" ? 0 : x.Severity == "warning" ? 1 : 2)
                .Take(30)
                .ToList();
        }

        private async Task<BusinessOptimizationSetting> GetDashboardSettingAsync(Guid businessId)
        {
            var fallback = new BusinessOptimizationSetting();
            if (businessId == Guid.Empty) return fallback;

            var setting = await db.tbl_business_dashboard_setting
                .AsNoTracking()
                .Where(x => !x.is_deleted && x.is_active && x.fk_business == businessId)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            if (setting == null) return fallback;

            return new BusinessOptimizationSetting
            {
                PeakStartTime = string.IsNullOrWhiteSpace(setting.peak_start_time) ? fallback.PeakStartTime : setting.peak_start_time,
                PeakEndTime = string.IsNullOrWhiteSpace(setting.peak_end_time) ? fallback.PeakEndTime : setting.peak_end_time,
                TimeZone = string.IsNullOrWhiteSpace(setting.timezone) ? fallback.TimeZone : setting.timezone,
                Currency = string.IsNullOrWhiteSpace(setting.currency) ? fallback.Currency : setting.currency,
                TariffRate = Convert.ToDouble(setting.tariff_rate),
                PeakTariffRate = Convert.ToDouble(setting.peak_tariff_rate),
                OffPeakTariffRate = Convert.ToDouble(setting.off_peak_tariff_rate)
            };
        }

        private async Task<Dictionary<Guid, ApplianceOptimizationMetadata>> GetApplianceOptimizationMetadataAsync(List<Guid> sensorIds)
        {
            if (!sensorIds.Any()) return new Dictionary<Guid, ApplianceOptimizationMetadata>();

            var rows = await (
                from assignment in db.tbl_sensor_appliance.AsNoTracking()
                join appliance in db.tbl_business_appliance.AsNoTracking() on assignment.fk_appliance equals appliance.business_appliance_id
                where sensorIds.Contains(assignment.fk_sensor)
                      && !assignment.is_deleted
                      && assignment.is_active
                      && !appliance.is_deleted
                      && appliance.is_active
                select new ApplianceOptimizationMetadata
                {
                    SensorId = assignment.fk_sensor,
                    ApplianceId = appliance.business_appliance_id,
                    ApplianceName = appliance.appliance_name,
                    IsCritical = appliance.is_critical,
                    PriorityLevel = appliance.priority_level
                })
                .ToListAsync();

            return rows
                .GroupBy(x => x.SensorId)
                .ToDictionary(g => g.Key, g => g.First());
        }

        private static List<OptimizationSuggestionDTO> BuildSuggestions(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, double> energy,
            double totalEnergy,
            int resetCount,
            List<IdleApplianceDTO> idleAppliances,
            List<FaultyApplianceDTO> faultyAppliances,
            List<PeakDemandHourDTO> peakDemandHours,
            PeakDemandSummaryDTO peakDemandSummary,
            List<EntityComparisonDTO> comparisons,
            List<UtilityConsumptionDTO> utilityBreakdown,
            BusinessOptimizationSetting dashboardSetting,
            Dictionary<Guid, ApplianceOptimizationMetadata> applianceMetadata,
            double selectedRangeHours)
        {
            var suggestions = new List<OptimizationSuggestionDTO>();
            var nowUtc = DateTime.UtcNow;
            var isCurrentPeakHour = IsPeakLocal(nowUtc, dashboardSetting);
            var tariffDelta = Math.Max(0, dashboardSetting.PeakTariffRate - dashboardSetting.OffPeakTariffRate);
            var topPeak = peakDemandHours.OrderByDescending(x => x.PeakPowerW).FirstOrDefault();

            if (totalEnergy <= 0 && latest.Count < Math.Max(1, chains.Count / 3))
            {
                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = "info",
                    Priority = "Low",
                    Type = "data-quality",
                    ReasonCode = "INSUFFICIENT_OPTIMIZATION_DATA",
                    Title = "More sensor data needed",
                    Message = "Optimization suggestions are limited because recent energy history or live Redis readings are missing.",
                    Action = "Keep sensors online, verify Redis latest readings, and review again after more data is collected."
                });
            }

            // Fault suggestions are always highest priority because they can indicate wiring, relay, sensor, or appliance issues.
            foreach (var faulty in faultyAppliances.Take(6))
            {
                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = "critical",
                    Priority = "High",
                    Type = "faulty-inspection",
                    ReasonCode = "FAULTY_APPLIANCE_CHECK",
                    Title = "Inspect possible faulty appliance",
                    SensorId = faulty.SensorId,
                    SensorName = faulty.SensorName,
                    ApplianceName = faulty.ApplianceName,
                    UtilityName = faulty.UtilityName,
                    OfficeName = faulty.OfficeName,
                    Message = faulty.Reason,
                    Action = faulty.RecommendedAction,
                    CanApplyAction = false
                });
            }

            // Peak demand suggestion with actual peak-hour conflict awareness.
            if (peakDemandHours.Any())
            {
                var averageHourlyPeak = peakDemandHours.Average(x => x.PeakPowerW);
                var peak = topPeak!;
                var peakIsConfiguredPeakHour = IsPeakLocal(peak.HourUtc, dashboardSetting);
                var avoidHours = string.IsNullOrWhiteSpace(peakDemandSummary.RecommendedAvoidHours)
                    ? peak.HourLabel
                    : peakDemandSummary.RecommendedAvoidHours;

                if (peak.PeakPowerW > averageHourlyPeak * 1.20 && peak.PeakPowerW >= 1000)
                {
                    var reviewCandidates = GetNonCriticalRunningSensors(chains, latest, applianceMetadata)
                        .OrderByDescending(x => x.LivePowerW)
                        .Take(4)
                        .ToList();

                    var estimatedShiftKwh = Math.Round(Math.Max(0.05, peak.EnergyKwh * 0.15), 2);
                    var estimatedSavingCost = Math.Round(estimatedShiftKwh * (tariffDelta > 0 ? tariffDelta : dashboardSetting.TariffRate), 2);
                    var action = reviewCandidates.Any()
                        ? $"Review or delay non-critical loads such as {string.Join(", ", reviewCandidates.Select(x => x.ApplianceName).Distinct().Take(3))} outside {avoidHours} where operationally safe."
                        : "Avoid starting HVAC, pumps, printers, and heavy computing loads together during this hour.";

                    suggestions.Add(new OptimizationSuggestionDTO
                    {
                        Severity = peakIsConfiguredPeakHour ? "warning" : "info",
                        Priority = peakIsConfiguredPeakHour ? "High" : "Medium",
                        Type = "peak-demand",
                        ReasonCode = peakIsConfiguredPeakHour ? "PEAK_DEMAND_OVERLAPS_TARIFF_PEAK" : "PEAK_DEMAND_SPIKE",
                        Title = peakIsConfiguredPeakHour ? "High demand during peak tariff window" : "Peak demand spike detected",
                        Message = $"Demand reached {peak.PeakPowerW:F0}W around {peak.HourLabel}. Average hourly peak is {averageHourlyPeak:F0}W.",
                        Action = action,
                        EstimatedSavingKwh = estimatedShiftKwh,
                        EstimatedSavingCost = estimatedSavingCost,
                        TimeBucket = peak.HourLabel,
                        CanApplyAction = false,
                        ConflictsWithPeakHour = peakIsConfiguredPeakHour
                    });
                }
            }

            // HVAC-focused optimization.
            var hvac = utilityBreakdown.FirstOrDefault(x => x.UtilityName.Equals("HVAC", StringComparison.OrdinalIgnoreCase));
            if (hvac != null && totalEnergy > 0 && hvac.EnergyKwh > 0)
            {
                var hvacSensors = chains
                    .Where(x => IsHvacChain(x.Value))
                    .Select(x => x.Key)
                    .ToList();

                var hvacLoopAvailable = hvacSensors.Any(id =>
                    chains.TryGetValue(id, out var chain)
                    && chain.HvacLoopSettingId.HasValue
                    && applianceMetadata.TryGetValue(id, out var meta)
                    && !meta.IsCritical);

                if (hvac.SharePercent >= 35 || (topPeak != null && hvac.CurrentPowerW >= Math.Max(1000, topPeak.PeakPowerW * 0.30)))
                {
                    var loopSavingKwh = Math.Round(Math.Max(0.05, hvac.EnergyKwh * (hvac.SharePercent >= 60 ? 0.20 : 0.12)), 2);
                    var savingRate = isCurrentPeakHour || (topPeak != null && IsPeakLocal(topPeak.HourUtc, dashboardSetting))
                        ? dashboardSetting.PeakTariffRate
                        : dashboardSetting.TariffRate;

                    suggestions.Add(new OptimizationSuggestionDTO
                    {
                        Severity = hvac.SharePercent >= 60 ? "warning" : "info",
                        Priority = hvac.SharePercent >= 60 ? "High" : "Medium",
                        Type = "hvac-peak-optimization",
                        ReasonCode = hvacLoopAvailable ? "HVAC_LOOP_RECOMMENDED" : "HVAC_HIGH_SHARE_REVIEW",
                        Title = hvacLoopAvailable ? "HVAC loop optimization available" : "HVAC consumption is high",
                        Message = $"HVAC is responsible for {hvac.SharePercent:F1}% of selected-range energy ({hvac.EnergyKwh:F2} kWh).",
                        Action = hvacLoopAvailable
                            ? "Apply the configured HVAC loop during peak hours with manual override protection."
                            : "Review HVAC setpoint, maintenance, staggered start timing, and configure an HVAC loop before using automatic loop control.",
                        EstimatedSavingKwh = loopSavingKwh,
                        EstimatedSavingCost = Math.Round(loopSavingKwh * savingRate, 2),
                        UtilityName = "HVAC",
                        CanApplyAction = hvacLoopAvailable,
                        ConflictsWithPeakHour = isCurrentPeakHour || (topPeak != null && IsPeakLocal(topPeak.HourUtc, dashboardSetting))
                    });
                }
            }

            // Idle/appliance standby savings.
            foreach (var idle in idleAppliances.Take(8))
            {
                var estimatedIdleHours = Math.Min(4, Math.Max(1, selectedRangeHours * 0.10));
                var savingKwh = Math.Round((Math.Max(0, idle.CurrentPowerW - idle.StandbyPowerW) * estimatedIdleHours) / 1000.0, 3);
                if (savingKwh <= 0) savingKwh = Math.Round((idle.CurrentPowerW * estimatedIdleHours) / 1000.0, 3);

                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = idle.CanTurnOff ? "warning" : "info",
                    Priority = idle.CanTurnOff ? "Medium" : "Low",
                    Type = idle.CanTurnOff ? "idle-auto-off" : "idle-policy",
                    ReasonCode = idle.CanTurnOff ? "IDLE_DEVICE_CAN_AUTO_OFF" : "IDLE_DEVICE_AUTO_OFF_DISABLED",
                    Title = idle.CanTurnOff ? "Turn off idle appliance" : "Enable standby auto-off",
                    SensorId = idle.SensorId,
                    SensorName = idle.SensorName,
                    ApplianceName = idle.ApplianceName,
                    UtilityName = idle.UtilityName,
                    OfficeName = idle.OfficeName,
                    Message = $"{idle.ApplianceName} is consuming {idle.CurrentPowerW:F0}W near standby while relay is ON.",
                    Action = idle.CanTurnOff
                        ? "Turn off this relay or let standby auto-off handle it automatically."
                        : "Enable standby auto-off for this sensor to reduce idle waste.",
                    EstimatedSavingKwh = savingKwh,
                    EstimatedSavingCost = Math.Round(savingKwh * dashboardSetting.TariffRate, 2),
                    CanApplyAction = idle.CanTurnOff
                });
            }

            // Tariff-based manual schedule review for non-critical live loads.
            var nonCriticalLiveLoads = GetNonCriticalRunningSensors(chains, latest, applianceMetadata)
                .Where(x => x.LivePowerW >= 40)
                .OrderByDescending(x => x.LivePowerW)
                .Take(6)
                .ToList();

            if (nonCriticalLiveLoads.Any() && (isCurrentPeakHour || peakDemandHours.Any(x => x.IsPeakHour && IsPeakLocal(x.HourUtc, dashboardSetting))))
            {
                var estimatedKwh = Math.Round(nonCriticalLiveLoads.Sum(x => x.LivePowerW) / 1000.0, 2);
                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = "warning",
                    Priority = "Medium",
                    Type = "tariff-schedule-review",
                    ReasonCode = "NON_CRITICAL_LOAD_DURING_PEAK",
                    Title = "Review non-critical load during peak hours",
                    Message = $"{nonCriticalLiveLoads.Count} non-critical appliance(s) are running or contributing during the peak window {dashboardSetting.PeakStartTime}-{dashboardSetting.PeakEndTime}.",
                    Action = $"Review the operating schedule for {string.Join(", ", nonCriticalLiveLoads.Select(x => x.ApplianceName).Distinct().Take(4))} and manually move usage outside peak hours where safe.",
                    EstimatedSavingKwh = estimatedKwh,
                    EstimatedSavingCost = Math.Round(estimatedKwh * tariffDelta, 2),
                    CanApplyAction = false,
                    ConflictsWithPeakHour = true
                });
            }

            // High consumers with contextual action.
            foreach (var hc in energy.OrderByDescending(x => x.Value).Take(7))
            {
                if (totalEnergy <= 0) continue;
                var share = (hc.Value / totalEnergy) * 100;
                if (share < 18) continue;
                chains.TryGetValue(hc.Key, out var chain);
                if (chain == null) continue;
                applianceMetadata.TryGetValue(hc.Key, out var meta);

                var action = meta?.IsCritical == false
                    ? BuildReviewAction(meta)
                    : "Compare with similar sensors/floors and check working hours, standby behavior, and appliance rating.";

                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = share >= 35 ? "warning" : "info",
                    Priority = share >= 35 ? "High" : "Medium",
                    Type = "high-consumption",
                    ReasonCode = meta?.IsCritical == false ? "HIGH_CONSUMER_NON_CRITICAL_REVIEW" : "HIGH_CONSUMER_REVIEW",
                    Title = "High energy consumer",
                    SensorId = hc.Key.ToString(),
                    SensorName = chain.SensorName,
                    ApplianceName = string.IsNullOrWhiteSpace(chain.ApplianceName) ? chain.SensorName : chain.ApplianceName,
                    UtilityName = chain.UtilityName,
                    OfficeName = chain.OfficeName,
                    Message = $"{chain.SensorName} consumed {hc.Value:F1} kWh ({share:F1}% of scope total).",
                    Action = action,
                    EstimatedSavingKwh = Math.Round(hc.Value * 0.08, 2),
                    EstimatedSavingCost = Math.Round(hc.Value * 0.08 * dashboardSetting.TariffRate, 2),
                    CanApplyAction = false
                });
            }

            // Comparison suggestions at hierarchy level.
            if (comparisons.Count > 1)
            {
                var average = comparisons.Average(x => x.EnergyKwh);
                var highest = comparisons.OrderByDescending(x => x.EnergyKwh).First();
                if (highest.EnergyKwh >= 5 && highest.EnergyKwh > average * 1.30)
                {
                    var excessKwh = Math.Round(Math.Max(0, highest.EnergyKwh - average), 2);
                    suggestions.Add(new OptimizationSuggestionDTO
                    {
                        Severity = "warning",
                        Priority = "Medium",
                        Type = "comparison",
                        ReasonCode = "AREA_ABOVE_AVERAGE",
                        Title = "Area is above average consumption",
                        Message = $"{highest.EntityName} uses {highest.EnergyKwh:F1} kWh, while average {highest.Level} usage is {average:F1} kWh.",
                        Action = "Review HVAC share, idle appliances, high-consumption sensors, and working-hour usage in this area.",
                        EstimatedSavingKwh = Math.Round(excessKwh * 0.15, 2),
                        EstimatedSavingCost = Math.Round(excessKwh * 0.15 * dashboardSetting.TariffRate, 2)
                    });
                }
            }

            // Metadata quality suggestions explain when prioritization cannot be applied.
            var metadataMissing = chains.Count(x =>
                !applianceMetadata.TryGetValue(x.Key, out var meta)
                || string.IsNullOrWhiteSpace(meta.PriorityLevel));

            if (metadataMissing > 0)
            {
                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = "info",
                    Priority = "Low",
                    Type = "metadata",
                    ReasonCode = "APPLIANCE_OPTIMIZATION_METADATA_MISSING",
                    Title = "Appliance priority settings need review",
                    Message = $"{metadataMissing} sensor/appliance mapping(s) have missing priority metadata.",
                    Action = "Set appliance priority and critical status so recommendations can be ranked safely."
                });
            }

            if (resetCount > 0)
            {
                suggestions.Add(new OptimizationSuggestionDTO
                {
                    Severity = "info",
                    Priority = "Low",
                    Type = "reset",
                    ReasonCode = "METER_RESET_HANDLED",
                    Title = "Meter reset handled",
                    Message = $"{resetCount} meter reset event(s) were detected. Dashboard used reset-safe delta calculation.",
                    Action = "Keep using delta-based energy calculation and store reset count for audit."
                });
            }

            return suggestions
                .GroupBy(x => new { x.ReasonCode, x.SensorId, x.TimeBucket, x.Title })
                .Select(g => g.First())
                .OrderBy(x => SeverityRank(x.Severity))
                .ThenBy(x => PriorityRank(x.Priority))
                .ThenByDescending(x => x.EstimatedSavingCost)
                .Take(25)
                .ToList();
        }

        private static List<EntityComparisonDTO> BuildComparisons(
            string level,
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, double> energy)
        {
            Func<SensorChainRedisDTO, (Guid id, string name, string level)> groupSelector = level switch
            {
                "business" => c => (c.FacilityId, c.FacilityName, "facility"),
                "facility" => c => (c.BuildingId, c.BuildingName, "building"),
                "building" => c => (c.FloorId, c.FloorName, "floor"),
                "floor" => c => (c.SectionId, c.SectionName, "section"),
                "section" => c => (c.OfficeId, c.OfficeName, "office"),
                "office" => c => (c.SensorId, c.SensorName, "sensor"),
                "device" => c => (c.SensorId, c.SensorName, "sensor"),
                _ => c => (c.SensorId, c.SensorName, "sensor")
            };

            var comparisons = chains.Values
                .GroupBy(groupSelector)
                .Select(g =>
                {
                    var sensorIds = g.Select(x => x.SensorId).ToList();
                    return new EntityComparisonDTO
                    {
                        EntityId = g.Key.id.ToString(),
                        EntityName = g.Key.name,
                        Level = g.Key.level,
                        EnergyKwh = Math.Round(sensorIds.Sum(id => energy.TryGetValue(id, out var value) ? value : 0), 2),
                        CurrentPowerW = Math.Round(sensorIds.Sum(id => latest.TryGetValue(id, out var value) ? value.ActivePower : 0), 1),
                        SensorCount = sensorIds.Count
                    };
                })
                .OrderByDescending(x => x.EnergyKwh)
                .ToList();

            if (comparisons.Count > 1)
            {
                var average = comparisons.Average(x => x.EnergyKwh);
                foreach (var item in comparisons)
                {
                    if (item.EnergyKwh >= 5 && item.EnergyKwh > average * 1.3)
                        item.Suggestion = $"{item.EntityName} is above average for {item.Level} energy consumption.";
                }
            }

            return comparisons;
        }

        private static bool IsHvacChain(SensorChainRedisDTO chain)
        {
            return string.Equals(chain.UtilityName, "HVAC", StringComparison.OrdinalIgnoreCase)
                || chain.ApplianceName.Contains("AC", StringComparison.OrdinalIgnoreCase)
                || chain.ApplianceName.Contains("HVAC", StringComparison.OrdinalIgnoreCase)
                || chain.SensorName.Contains("AC", StringComparison.OrdinalIgnoreCase)
                || chain.SensorName.Contains("HVAC", StringComparison.OrdinalIgnoreCase);
        }

        private static List<RunningReviewCandidate> GetNonCriticalRunningSensors(
            Dictionary<Guid, SensorChainRedisDTO> chains,
            Dictionary<Guid, SensorLatestRedisDTO> latest,
            Dictionary<Guid, ApplianceOptimizationMetadata> applianceMetadata)
        {
            var result = new List<RunningReviewCandidate>();

            foreach (var item in latest)
            {
                if (!chains.TryGetValue(item.Key, out var chain)) continue;
                if (!applianceMetadata.TryGetValue(item.Key, out var meta)) continue;
                if (meta.IsCritical) continue;

                var power = Math.Max(0, item.Value.ActivePower);
                var standbyLimit = Math.Max(chain.StandbyPower * 1.5, chain.StandbyPower + 8);
                if (power <= Math.Max(20, standbyLimit)) continue;

                result.Add(new RunningReviewCandidate
                {
                    SensorId = item.Key,
                    SensorName = chain.SensorName,
                    ApplianceName = string.IsNullOrWhiteSpace(meta.ApplianceName) ? chain.ApplianceName : meta.ApplianceName,
                    UtilityName = chain.UtilityName,
                    OfficeName = chain.OfficeName,
                    LivePowerW = power
                });
            }

            return result;
        }

        private static string BuildReviewAction(ApplianceOptimizationMetadata meta)
        {
            if (meta.IsCritical)
                return "This appliance is marked critical. Review consumption manually without interrupting operation.";

            var priority = string.IsNullOrWhiteSpace(meta.PriorityLevel) ? "Normal" : meta.PriorityLevel;
            return $"Review working hours, standby behavior, and operating schedule for this {priority.ToLowerInvariant()}-priority non-critical appliance.";
        }

        private static bool IsPeakLocal(DateTime utcValue, BusinessOptimizationSetting setting)
        {
            var local = ConvertUtcToBusinessLocal(utcValue, setting.TimeZone);
            var current = local.TimeOfDay;
            var start = ParseTimeOfDay(setting.PeakStartTime, new TimeSpan(18, 0, 0));
            var end = ParseTimeOfDay(setting.PeakEndTime, new TimeSpan(23, 0, 0));

            if (start <= end)
                return current >= start && current <= end;

            // Overnight peak window, for example 22:00 to 02:00.
            return current >= start || current <= end;
        }

        private static DateTime ConvertUtcToBusinessLocal(DateTime utcValue, string timeZoneId)
        {
            var utc = utcValue.Kind == DateTimeKind.Utc
                ? utcValue
                : DateTime.SpecifyKind(utcValue, DateTimeKind.Utc);

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(timeZoneId) ? "Asia/Karachi" : timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            }
            catch
            {
                return utc.AddHours(5); // Safe fallback for Pakistan if OS timezone lookup fails.
            }
        }

        private static TimeSpan ParseTimeOfDay(string value, TimeSpan fallback)
        {
            return TimeSpan.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static int SeverityRank(string severity)
        {
            return severity?.ToLowerInvariant() switch
            {
                "critical" => 0,
                "warning" => 1,
                "info" => 2,
                "success" => 3,
                _ => 4
            };
        }

        private static int PriorityRank(string priority)
        {
            return priority?.ToLowerInvariant() switch
            {
                "high" => 0,
                "medium" => 1,
                "low" => 2,
                _ => 3
            };
        }

        private static DateTime TruncateToHourUtc(DateTime value)
        {
            var utc = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
        }

        private class ResolvedOptimizationRange
        {
            public DateTime FromUtc { get; set; }
            public DateTime ToUtc { get; set; }
            public string Range { get; set; } = "24h";
            public bool IsCustomRange { get; set; }
            public string RangeLabel { get; set; } = string.Empty;
        }

        private class OptimizationReadingRow
        {
            public Guid SensorId { get; set; }
            public DateTime CreatedAt { get; set; }
            public double ActiveEnergy { get; set; }
            public double ActivePower { get; set; }
            public double PowerFactor { get; set; }
            public double Voltage { get; set; }
            public double Current { get; set; }
        }

        private class BusinessOptimizationSetting
        {
            public string PeakStartTime { get; set; } = "18:00";
            public string PeakEndTime { get; set; } = "23:00";
            public string TimeZone { get; set; } = "Asia/Karachi";
            public string Currency { get; set; } = "PKR";
            public double TariffRate { get; set; } = 65;
            public double PeakTariffRate { get; set; } = 75;
            public double OffPeakTariffRate { get; set; } = 55;
        }

        private class ApplianceOptimizationMetadata
        {
            public Guid SensorId { get; set; }
            public Guid ApplianceId { get; set; }
            public string ApplianceName { get; set; } = string.Empty;
            public bool IsCritical { get; set; }
            public string PriorityLevel { get; set; } = "Normal";
        }

        private class RunningReviewCandidate
        {
            public Guid SensorId { get; set; }
            public string SensorName { get; set; } = string.Empty;
            public string ApplianceName { get; set; } = string.Empty;
            public string UtilityName { get; set; } = string.Empty;
            public string OfficeName { get; set; } = string.Empty;
            public double LivePowerW { get; set; }
        }
    }
}
