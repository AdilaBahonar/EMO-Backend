using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.RedisRuntimeDTOs;
using EMO.Repositories.EnergyDashboardServicesRepo;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace EMO.Repositories.EnergyDashboardRepo
{
    public class EnergyDashboardService : IEnergyDashboardService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase? redis;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(65);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> LiveAggregateLocks = new();
        private static readonly JsonSerializerOptions RedisJsonOptions = new() { PropertyNameCaseInsensitive = true };
        private const int PakistanUtcOffsetHours = 5;
        private const int MinimumOnlineThresholdSeconds = 180;
        private const int DelayedThresholdSeconds = 600;
        private const string LiveTodayGranularity = "crm-live-today";

        public EnergyDashboardService(DBUserManagementContext db, IConnectionMultiplexer? connectionMultiplexer = null)
        {
            this.db = db;
            redis = connectionMultiplexer?.GetDatabase();
        }

        public async Task<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>> GetMonthlyDeviceTypeReport(string? businessId = null, string? tenantId = null)
        {
            try
            {
                var toDate = DateTime.UtcNow;
                var fromDate = toDate.AddDays(-30);
                var businessGuid = TryParseGuid(businessId);
                var tenantGuid = TryParseGuid(tenantId);
                var officeIds = tenantGuid.HasValue
                    ? await GetTenantOfficeIdsAsync(tenantGuid.Value, businessGuid)
                    : null;
                var cacheKey = BuildScopedCacheKey("dashboard:legacy:monthly-device-type:last30", businessGuid, tenantGuid);
                var cached = await GetCacheAsync<List<MonthlyDeviceTypeReportResponseDTO>>(cacheKey);
                if (cached != null)
                {
                    return Success(cached);
                }

                var rows = await GetReadingRowsAsync(fromDate, toDate, businessGuid, officeIds);
                var utilityTotals = CalculateConsumptionBySensor(rows)
                    .GroupBy(x => x.utilityName)
                    .Select(g => new
                    {
                        utilityName = g.Key,
                        totalKwh = g.Sum(x => x.totalKwh)
                    })
                    .OrderByDescending(x => x.totalKwh)
                    .ToList();

                var total = utilityTotals.Sum(x => x.totalKwh);
                var result = utilityTotals.Select(x => new MonthlyDeviceTypeReportResponseDTO
                {
                    utilityName = x.utilityName,
                    totalKwh = (float)Math.Round(x.totalKwh, 2),
                    percentage = total > 0 ? (float)Math.Round((x.totalKwh / total) * 100, 2) : 0
                }).ToList();

                await SetCacheAsync(cacheKey, result);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Failure<List<MonthlyDeviceTypeReportResponseDTO>>(ex);
            }
        }

        public async Task<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>> GetEnergyConsumptionByDeviceTypeLast12Months(string? businessId = null, string? tenantId = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var fromDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);
                var businessGuid = TryParseGuid(businessId);
                var tenantGuid = TryParseGuid(tenantId);
                var officeIds = tenantGuid.HasValue
                    ? await GetTenantOfficeIdsAsync(tenantGuid.Value, businessGuid)
                    : null;
                var cacheKey = BuildScopedCacheKey($"dashboard:legacy:utility-consumption:last12:{fromDate:yyyyMM}:{now:yyyyMMddHH}", businessGuid, tenantGuid);
                var cached = await GetCacheAsync<List<EnergyConsumptionByDeviceTypeResponseDTO>>(cacheKey);
                if (cached != null)
                {
                    return Success(cached);
                }

                var rows = await GetReadingRowsAsync(fromDate, now, businessGuid, officeIds);

                var result = CalculateConsumptionBySensor(rows, "month")
                    .GroupBy(x => new
                    {
                        x.bucketStart.Year,
                        x.bucketStart.Month,
                        x.utilityName
                    })
                    .Select(g => new EnergyConsumptionByDeviceTypeResponseDTO
                    {
                        year = g.Key.Year,
                        month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM", CultureInfo.InvariantCulture),
                        utilityName = g.Key.utilityName,
                        totalKwh = (float)Math.Round(g.Sum(x => x.totalKwh), 2)
                    })
                    .OrderBy(x => x.year)
                    .ThenBy(x => DateTime.ParseExact(x.month, "MMM", CultureInfo.InvariantCulture).Month)
                    .ToList();

                await SetCacheAsync(cacheKey, result);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Failure<List<EnergyConsumptionByDeviceTypeResponseDTO>>(ex);
            }
        }

        public async Task<ResponseModel<PeakNonPeakSummaryResponseDTO>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null)
        {
            try
            {
                var fromDate = EnsureUtc(startDate.Date);
                var toDate = EnsureUtc(endDate.Date.AddDays(1).AddTicks(-1));
                var businessGuid = TryParseGuid(businessId);
                var tenantGuid = TryParseGuid(tenantId);
                var officeIds = tenantGuid.HasValue
                    ? await GetTenantOfficeIdsAsync(tenantGuid.Value, businessGuid)
                    : null;
                var settingBusinessId = businessGuid ?? await ResolveBusinessIdForTenantAsync(tenantGuid, officeIds);
                var setting = await GetDashboardSettingAsync(settingBusinessId);

                var rows = await GetReadingRowsAsync(fromDate, toDate, businessGuid, officeIds);
                var dailyTotals = new Dictionary<DateTime, (double peakKwh, double nonPeakKwh)>();

                foreach (var item in CalculateIntervalConsumption(rows))
                {
                    var localTime = ToPakistanLocal(item.currentCreatedAt);
                    var localDay = localTime.Date;
                    var isPeak = IsPeakLocal(localTime, setting);
                    if (!dailyTotals.ContainsKey(localDay))
                    {
                        dailyTotals[localDay] = (0, 0);
                    }

                    var current = dailyTotals[localDay];
                    if (isPeak)
                    {
                        current.peakKwh += item.consumptionKwh;
                    }
                    else
                    {
                        current.nonPeakKwh += item.consumptionKwh;
                    }
                    dailyTotals[localDay] = current;
                }

                var dailyData = dailyTotals
                    .Select(x =>
                    {
                        var totalKwh = x.Value.peakKwh + x.Value.nonPeakKwh;
                        return new PeakNonPeakAnalysisResponseDTO
                        {
                            period = x.Key.ToString("yyyy-MM-dd"),
                            peakKwh = (float)Math.Round(x.Value.peakKwh, 2),
                            nonPeakKwh = (float)Math.Round(x.Value.nonPeakKwh, 2),
                            totalKwh = (float)Math.Round(totalKwh, 2),
                            peakPercentage = totalKwh > 0 ? (float)Math.Round((x.Value.peakKwh / totalKwh) * 100, 2) : 0,
                            nonPeakPercentage = totalKwh > 0 ? (float)Math.Round((x.Value.nonPeakKwh / totalKwh) * 100, 2) : 0
                        };
                    })
                    .OrderBy(x => x.period)
                    .ToList();

                var totalPeak = dailyData.Sum(x => x.peakKwh);
                var totalNonPeak = dailyData.Sum(x => x.nonPeakKwh);
                var total = totalPeak + totalNonPeak;

                return Success(new PeakNonPeakSummaryResponseDTO
                {
                    totalPeakKwh = (float)Math.Round(totalPeak, 2),
                    totalNonPeakKwh = (float)Math.Round(totalNonPeak, 2),
                    totalKwh = (float)Math.Round(total, 2),
                    peakPercentage = total > 0 ? (float)Math.Round((totalPeak / total) * 100, 2) : 0,
                    nonPeakPercentage = total > 0 ? (float)Math.Round((totalNonPeak / total) * 100, 2) : 0,
                    peakStartTime = setting.peak_start_time,
                    peakEndTime = setting.peak_end_time,
                    dailyData = dailyData
                });
            }
            catch (Exception ex)
            {
                return Failure<PeakNonPeakSummaryResponseDTO>(ex);
            }
        }

        public async Task<byte[]> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate, string? businessId = null, string? tenantId = null)
        {
            var response = await GetPeakNonPeakAnalysis(startDate, endDate, businessId, tenantId);
            var csv = new StringBuilder();
            csv.AppendLine("Period,Peak kWh,Non-Peak kWh,Total kWh,Peak %,Non-Peak %");

            foreach (var row in response.data?.dailyData ?? new List<PeakNonPeakAnalysisResponseDTO>())
            {
                csv.AppendLine($"{row.period},{row.peakKwh},{row.nonPeakKwh},{row.totalKwh},{row.peakPercentage},{row.nonPeakPercentage}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportEnergyConsumptionByDeviceTypeCsv(string? businessId = null, string? tenantId = null)
        {
            var response = await GetEnergyConsumptionByDeviceTypeLast12Months(businessId, tenantId);
            var csv = new StringBuilder();
            csv.AppendLine("Year,Month,Utility,Total kWh");

            foreach (var row in response.data ?? new List<EnergyConsumptionByDeviceTypeResponseDTO>())
            {
                csv.AppendLine($"{row.year},{row.month},{row.utilityName},{row.totalKwh}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<ResponseModel<CrmDashboardSummaryResponseDTO>> GetBusinessDashboardSummary(Guid businessId)
        {
            try
            {
                var range = ResolveDashboardRange("30d", null, null);
                return Success(await BuildAndStoreSummaryAsync(businessId, null, null, range));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardSummaryResponseDTO>(ex);
            }
        }

        public async Task<ResponseModel<CrmDashboardSummaryResponseDTO>> GetTenantDashboardSummary(Guid tenantId, Guid? businessId = null)
        {
            try
            {
                var officeIds = await GetTenantOfficeIdsAsync(tenantId, businessId);
                var resolvedBusinessId = businessId ?? await ResolveBusinessIdForTenantAsync(tenantId, officeIds) ?? Guid.Empty;
                var range = ResolveDashboardRange("30d", null, null);
                return Success(await BuildAndStoreSummaryAsync(resolvedBusinessId, tenantId, officeIds, range));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardSummaryResponseDTO>(ex);
            }
        }

        public async Task<ResponseModel<CrmDashboardChartResponseDTO>> GetBusinessDashboardChart(Guid businessId, string chartType, string range = "30d", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var resolvedRange = ResolveDashboardRange(range, fromDate, toDate);
                return Success(await BuildAndStoreChartAsync(businessId, null, null, chartType, resolvedRange));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardChartResponseDTO>(ex);
            }
        }

        public async Task<ResponseModel<CrmDashboardChartResponseDTO>> GetTenantDashboardChart(Guid tenantId, string chartType, string range = "30d", DateTime? fromDate = null, DateTime? toDate = null, Guid? businessId = null)
        {
            try
            {
                var officeIds = await GetTenantOfficeIdsAsync(tenantId, businessId);
                var resolvedBusinessId = businessId ?? await ResolveBusinessIdForTenantAsync(tenantId, officeIds) ?? Guid.Empty;
                var resolvedRange = ResolveDashboardRange(range, fromDate, toDate);
                return Success(await BuildAndStoreChartAsync(resolvedBusinessId, tenantId, officeIds, chartType, resolvedRange));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardChartResponseDTO>(ex);
            }
        }

        public async Task<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>> GetBusinessDashboardSuggestions(Guid businessId)
        {
            try
            {
                // Suggestions are generated continuously by the Optimization Worker.
                // Dashboard APIs are read-only so opening or refreshing the UI cannot create suggestions.
                return Success(await ReadActiveSuggestionsAsync(businessId, null, null));
            }
            catch (Exception ex)
            {
                return Failure<List<CrmDashboardSuggestionResponseDTO>>(ex);
            }
        }

        public async Task<ResponseModel<List<CrmDashboardSuggestionResponseDTO>>> GetTenantDashboardSuggestions(Guid tenantId, Guid? businessId = null)
        {
            try
            {
                var officeIds = await GetTenantOfficeIdsAsync(tenantId, businessId);
                var resolvedBusinessId = businessId ?? await ResolveBusinessIdForTenantAsync(tenantId, officeIds) ?? Guid.Empty;
                return Success(await ReadActiveSuggestionsAsync(resolvedBusinessId, tenantId, officeIds));
            }
            catch (Exception ex)
            {
                return Failure<List<CrmDashboardSuggestionResponseDTO>>(ex);
            }
        }

        public async Task<ResponseModel<CrmDashboardLiveOverviewResponseDTO>> GetBusinessDashboardLiveOverview(Guid businessId, bool forceRefresh = false)
        {
            try
            {
                return Success(await BuildLiveOverviewAsync(businessId, null, null, forceRefresh));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardLiveOverviewResponseDTO>(ex);
            }
        }

        public async Task<ResponseModel<CrmDashboardLiveOverviewResponseDTO>> GetTenantDashboardLiveOverview(Guid tenantId, Guid? businessId = null, bool forceRefresh = false)
        {
            try
            {
                var officeIds = await GetTenantOfficeIdsAsync(tenantId, businessId);
                var resolvedBusinessId = businessId ?? await ResolveBusinessIdForTenantAsync(tenantId, officeIds) ?? Guid.Empty;
                return Success(await BuildLiveOverviewAsync(resolvedBusinessId, tenantId, officeIds, forceRefresh));
            }
            catch (Exception ex)
            {
                return Failure<CrmDashboardLiveOverviewResponseDTO>(ex);
            }
        }

        private async Task<CrmDashboardLiveOverviewResponseDTO> BuildLiveOverviewAsync(
            Guid businessId,
            Guid? tenantId,
            List<Guid>? officeIds,
            bool forceRefresh)
        {
            var nowUtc = DateTime.UtcNow;
            var sensors = await GetLiveScopeSensorsAsync(businessId, tenantId, officeIds);
            var sensorIds = sensors.Select(x => x.sensorId).Distinct().ToList();
            var latestReadings = await GetLatestSensorReadingsAsync(sensorIds, nowUtc);
            var configuration = await GetLiveDashboardConfigurationAsync(businessId, forceRefresh);
            var onlineThresholdSeconds = Math.Max(
                MinimumOnlineThresholdSeconds,
                configuration.onlineThresholdSeconds);

            var onlineCutoff = nowUtc.AddSeconds(-onlineThresholdSeconds);
            var delayedCutoff = nowUtc.AddSeconds(-DelayedThresholdSeconds);

            var online = new List<(LiveScopeSensor sensor, SensorLatestRedisDTO reading)>();
            var delayedCount = 0;
            var offlineCount = 0;
            var neverConnectedCount = 0;

            foreach (var sensor in sensors)
            {
                if (!latestReadings.TryGetValue(sensor.sensorId, out var reading))
                {
                    neverConnectedCount++;
                    continue;
                }

                var receivedAtUtc = EnsureUtc(reading.ReceivedAtUtc);
                if (receivedAtUtc >= onlineCutoff)
                {
                    online.Add((sensor, reading));
                }
                else if (receivedAtUtc >= delayedCutoff)
                {
                    delayedCount++;
                }
                else
                {
                    offlineCount++;
                }
            }

            var currentLoadW = online.Sum(x => Math.Max(0, x.reading.ActivePower));
            var utilityLoads = online
                .GroupBy(x => string.IsNullOrWhiteSpace(x.sensor.utilityName) ? "Unassigned" : x.sensor.utilityName)
                .Select(group => new CrmLiveUtilityLoadResponseDTO
                {
                    utilityName = group.Key,
                    currentLoadW = Math.Round(group.Sum(x => Math.Max(0, x.reading.ActivePower)), 2),
                    onlineSensors = group.Select(x => x.sensor.sensorId).Distinct().Count()
                })
                .OrderByDescending(x => x.currentLoadW)
                .ToList();

            foreach (var utility in utilityLoads)
            {
                utility.percentage = currentLoadW > 0
                    ? Math.Round((utility.currentLoadW / currentLoadW) * 100, 1)
                    : 0;
            }

            var topConsumers = online
                .OrderByDescending(x => Math.Max(0, x.reading.ActivePower))
                .Take(8)
                .Select(x => new CrmLiveConsumerResponseDTO
                {
                    sensorId = x.sensor.sensorId.ToString(),
                    sensorName = x.sensor.sensorName,
                    applianceName = x.sensor.applianceName,
                    utilityName = string.IsNullOrWhiteSpace(x.sensor.utilityName) ? "Unassigned" : x.sensor.utilityName,
                    officeName = x.sensor.officeName,
                    floorName = x.sensor.floorName,
                    currentLoadW = Math.Round(Math.Max(0, x.reading.ActivePower), 2),
                    voltage = Math.Round(Math.Max(0, x.reading.Voltage), 1),
                    powerFactor = Math.Round(Math.Max(0, x.reading.PowerFactor), 2),
                    relayState = x.reading.RelayState,
                    receivedAtUtc = EnsureUtc(x.reading.ReceivedAtUtc).ToString("o")
                })
                .ToList();

            var assignedSensorIds = sensors
                .Where(x => x.hasApplianceAssignment)
                .Select(x => x.sensorId)
                .Distinct()
                .ToHashSet();
            var configuredSensorIds = sensors
                .Where(x => x.hasOptimizationProfile)
                .Select(x => x.sensorId)
                .Distinct()
                .ToHashSet();

            var assignmentCoverage = sensors.Count > 0
                ? (double)assignedSensorIds.Count / sensors.Count
                : 0;
            var optimizationCoverage = sensors.Count > 0
                ? (double)configuredSensorIds.Count / sensors.Count
                : 0;
            var readiness =
                (configuration.tariffConfigured ? 25 : 0) +
                (configuration.demandLimitConfigured ? 25 : 0) +
                (int)Math.Round(assignmentCoverage * 25) +
                (int)Math.Round(optimizationCoverage * 25);

            var todayAggregate = await GetOrBuildTodayAggregateAsync(
                businessId,
                tenantId,
                officeIds,
                configuration.tariffRatePerKwh,
                configuration.tariffConfigured,
                nowUtc,
                forceRefresh);

            var voltageValues = online
                .Select(x => x.reading.Voltage)
                .Where(x => x > 0)
                .ToList();
            var powerFactorValues = online
                .Where(x => x.reading.ActivePower >= 50 && x.reading.PowerFactor > 0)
                .Select(x => x.reading.PowerFactor)
                .ToList();
            var lastLiveUpdate = latestReadings.Count > 0
                ? latestReadings.Values.Max(x => EnsureUtc(x.ReceivedAtUtc))
                : (DateTime?)null;

            return new CrmDashboardLiveOverviewResponseDTO
            {
                energyTodayKwh = Math.Round(todayAggregate.energyKwh, 2),
                estimatedCostToday = configuration.tariffConfigured
                    ? Math.Round(todayAggregate.estimatedCost, 2)
                    : null,
                costConfigured = configuration.tariffConfigured,
                currentLoadW = Math.Round(currentLoadW, 2),
                peakDemandTodayW = Math.Round(todayAggregate.peakDemandW, 2),
                totalSensors = sensors.Count,
                onlineSensors = online.Count,
                delayedSensors = delayedCount,
                offlineSensors = offlineCount,
                neverConnectedSensors = neverConnectedCount,
                averageVoltage = voltageValues.Count > 0 ? Math.Round(voltageValues.Average(), 1) : 0,
                averagePowerFactor = powerFactorValues.Count > 0 ? Math.Round(powerFactorValues.Average(), 2) : 0,
                assignedSensors = assignedSensorIds.Count,
                configuredOptimizationSensors = configuredSensorIds.Count,
                optimizationReadinessPercent = Math.Clamp(readiness, 0, 100),
                tariffConfigured = configuration.tariffConfigured,
                demandLimitConfigured = configuration.demandLimitConfigured,
                demandLimitKw = configuration.demandLimitKw,
                onlineThresholdSeconds = onlineThresholdSeconds,
                delayedThresholdSeconds = DelayedThresholdSeconds,
                liveUpdatedAtUtc = lastLiveUpdate?.ToString("o") ?? string.Empty,
                aggregateUpdatedAtUtc = todayAggregate.updatedAtUtc.ToString("o"),
                utilityLoads = utilityLoads,
                topConsumers = topConsumers
            };
        }

        private async Task<List<LiveScopeSensor>> GetLiveScopeSensorsAsync(Guid businessId, Guid? tenantId, List<Guid>? officeIds)
        {
            var cacheKey = tenantId.HasValue
                ? $"dashboard:crm:live-metadata:tenant:{tenantId.Value}:business:{businessId}"
                : $"dashboard:crm:live-metadata:business:{businessId}";
            var cached = await GetCacheAsync<List<LiveScopeSensor>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var query =
                from sensor in db.tbl_sensor.AsNoTracking()
                join device in db.tbl_device.AsNoTracking() on sensor.fk_device equals device.device_id
                join office in db.tbl_office.AsNoTracking() on device.fk_office equals office.office_id into officeJoin
                from office in officeJoin.DefaultIfEmpty()
                join section in db.tbl_section.AsNoTracking() on office.fk_section equals section.section_id into sectionJoin
                from section in sectionJoin.DefaultIfEmpty()
                join floor in db.tbl_floor.AsNoTracking() on section.fk_floor equals floor.floor_id into floorJoin
                from floor in floorJoin.DefaultIfEmpty()
                join utility in db.tbl_utility.AsNoTracking() on sensor.fk_utility equals utility.utility_id into utilityJoin
                from utility in utilityJoin.DefaultIfEmpty()
                where !sensor.is_deleted
                      && !device.is_deleted
                      && device.fk_business == businessId
                select new LiveScopeSensor
                {
                    sensorId = sensor.sensor_id,
                    sensorName = sensor.sensor_name,
                    officeId = device.fk_office,
                    officeName = office != null ? office.office_name : string.Empty,
                    floorName = floor != null ? floor.floor_name : string.Empty,
                    utilityName = utility != null ? utility.utility_name : "Unassigned"
                };

            if (officeIds != null)
            {
                query = query.Where(x => officeIds.Contains(x.officeId));
            }

            var sensors = await query.ToListAsync();
            if (sensors.Count == 0)
            {
                return sensors;
            }

            var sensorIds = sensors.Select(x => x.sensorId).Distinct().ToList();
            var assignments = await (
                from assignment in db.tbl_sensor_appliance.AsNoTracking()
                join appliance in db.tbl_business_appliance.AsNoTracking()
                    on assignment.fk_appliance equals appliance.business_appliance_id
                where sensorIds.Contains(assignment.fk_sensor)
                      && assignment.is_active
                      && !assignment.is_deleted
                      && appliance.is_active
                      && !appliance.is_deleted
                select new
                {
                    sensorId = assignment.fk_sensor,
                    appliance.appliance_name,
                    appliance.priority_level,
                    appliance.is_critical,
                    appliance.rated_voltage,
                    appliance.min_current,
                    appliance.max_current,
                    appliance.min_power,
                    appliance.max_power,
                    appliance.standby_power,
                    appliance.normal_power_factor
                })
                .ToListAsync();

            var assignmentMap = assignments
                .GroupBy(x => x.sensorId)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var sensor in sensors)
            {
                if (!assignmentMap.TryGetValue(sensor.sensorId, out var assignment))
                {
                    continue;
                }

                sensor.hasApplianceAssignment = true;
                sensor.applianceName = assignment.appliance_name;
                sensor.hasOptimizationProfile = HasCompleteOptimizationProfile(
                    assignment.priority_level,
                    assignment.rated_voltage,
                    assignment.min_current,
                    assignment.max_current,
                    assignment.min_power,
                    assignment.max_power,
                    assignment.standby_power,
                    assignment.normal_power_factor);
            }

            await SetCacheAsync(cacheKey, sensors, TimeSpan.FromMinutes(5));
            return sensors;
        }

        private static bool HasCompleteOptimizationProfile(
            string? priorityLevel,
            double ratedVoltage,
            double minCurrent,
            double maxCurrent,
            double minPower,
            double maxPower,
            double standbyPower,
            double normalPowerFactor)
        {
            return !string.IsNullOrWhiteSpace(priorityLevel)
                   && ratedVoltage > 0
                   && minCurrent >= 0
                   && maxCurrent > 0
                   && minCurrent <= maxCurrent
                   && minPower >= 0
                   && maxPower > 0
                   && minPower <= maxPower
                   && standbyPower >= 0
                   && standbyPower <= maxPower
                   && normalPowerFactor > 0
                   && normalPowerFactor <= 1;
        }

        private static bool IsSafeOptimizationCandidate(
            string? priorityLevel,
            bool isCritical,
            double ratedVoltage,
            double minCurrent,
            double maxCurrent,
            double minPower,
            double maxPower,
            double standbyPower,
            double normalPowerFactor)
        {
            if (isCritical || !HasCompleteOptimizationProfile(
                    priorityLevel,
                    ratedVoltage,
                    minCurrent,
                    maxCurrent,
                    minPower,
                    maxPower,
                    standbyPower,
                    normalPowerFactor))
            {
                return false;
            }

            return string.Equals(priorityLevel, "Low", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(priorityLevel, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<Guid, SensorLatestRedisDTO>> GetLatestSensorReadingsAsync(
            List<Guid> sensorIds,
            DateTime nowUtc)
        {
            var result = new Dictionary<Guid, SensorLatestRedisDTO>();
            if (sensorIds.Count == 0)
            {
                return result;
            }

            if (redis != null)
            {
                try
                {
                    var keys = sensorIds
                        .Select(id => (RedisKey)$"sensor:latest:{id}")
                        .ToArray();
                    var values = await redis.StringGetAsync(keys);

                    for (var index = 0; index < sensorIds.Count; index++)
                    {
                        if (!values[index].HasValue)
                        {
                            continue;
                        }

                        var reading = JsonSerializer.Deserialize<SensorLatestRedisDTO>(values[index]!, RedisJsonOptions);
                        if (reading != null)
                        {
                            result[sensorIds[index]] = reading;
                        }
                    }
                }
                catch
                {
                    // Fall through to the recent database readings below.
                }
            }

            var missingIds = sensorIds.Where(id => !result.ContainsKey(id)).ToList();
            if (missingIds.Count == 0)
            {
                return result;
            }

            var fallbackCutoff = nowUtc.AddMinutes(-30);
            var fallbackRows = await db.tbl_singal_phase_data
                .AsNoTracking()
                .Where(x => missingIds.Contains(x.fk_sensor)
                            && !x.is_deleted
                            && x.created_at >= fallbackCutoff)
                .OrderByDescending(x => x.created_at)
                .Select(x => new
                {
                    x.fk_sensor,
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

            foreach (var row in fallbackRows.GroupBy(x => x.fk_sensor).Select(x => x.First()))
            {
                result[row.fk_sensor] = new SensorLatestRedisDTO
                {
                    SensorId = row.fk_sensor,
                    ReceivedAtUtc = EnsureUtc(row.created_at),
                    Voltage = row.volt,
                    Current = row.current,
                    ActivePower = row.active_power,
                    ReactivePower = row.reactive_power,
                    ApparentPower = row.apperent_power,
                    PowerFactor = row.power_factor,
                    Frequency = row.frequency,
                    ActiveEnergy = row.active_energy,
                    ReactiveEnergy = row.reactive_energy
                };
            }

            return result;
        }

        private async Task<LiveDashboardConfiguration> GetLiveDashboardConfigurationAsync(Guid businessId, bool forceRefresh = false)
        {
            var cacheKey = $"dashboard:crm:live-config:business:{businessId}";
            var cached = forceRefresh ? null : await GetCacheAsync<LiveDashboardConfiguration>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var tariff = await db.tbl_energy_tariff_plan
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var legacySetting = await db.tbl_business_dashboard_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var demand = await db.tbl_demand_management_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var tariffRate = tariff != null && tariff.standard_rate_per_kwh > 0
                ? (double)tariff.standard_rate_per_kwh
                : legacySetting != null && legacySetting.tariff_rate > 0
                    ? (double)legacySetting.tariff_rate
                    : 0;

            var result = new LiveDashboardConfiguration
            {
                tariffConfigured = tariffRate > 0,
                tariffRatePerKwh = tariffRate,
                demandLimitConfigured = demand != null && demand.demand_limit_kw > 0,
                demandLimitKw = demand != null && demand.demand_limit_kw > 0
                    ? (double)demand.demand_limit_kw
                    : null,
                onlineThresholdSeconds = legacySetting?.online_sensor_threshold_seconds
                    ?? MinimumOnlineThresholdSeconds
            };

            await SetCacheAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        private async Task<LiveDayAggregate> GetOrBuildTodayAggregateAsync(
            Guid businessId,
            Guid? tenantId,
            List<Guid>? officeIds,
            double tariffRatePerKwh,
            bool tariffConfigured,
            DateTime nowUtc,
            bool forceRefresh)
        {
            var pakistanNow = nowUtc.AddHours(PakistanUtcOffsetHours);
            var localDayStart = new DateTime(
                pakistanNow.Year,
                pakistanNow.Month,
                pakistanNow.Day,
                0,
                0,
                0,
                DateTimeKind.Unspecified);
            var dayStartUtc = DateTime.SpecifyKind(localDayStart.AddHours(-PakistanUtcOffsetHours), DateTimeKind.Utc);
            var dayEndUtc = dayStartUtc.AddDays(1);
            var freshnessCutoff = nowUtc.AddMinutes(-55);
            var cacheKey = tenantId.HasValue
                ? $"dashboard:crm:live-day:tenant:{tenantId.Value}:business:{businessId}:{dayStartUtc:yyyyMMdd}"
                : $"dashboard:crm:live-day:business:{businessId}:{dayStartUtc:yyyyMMdd}";
            var cached = forceRefresh ? null : await GetCacheAsync<LiveDayAggregate>(cacheKey);
            if (cached != null && cached.updatedAtUtc >= freshnessCutoff)
            {
                return cached;
            }

            var existing = await db.tbl_dashboard_aggregate
                .AsNoTracking()
                .Where(x => x.fk_business == businessId
                            && x.fk_tenant == tenantId
                            && x.granularity == LiveTodayGranularity
                            && x.from_time == dayStartUtc
                            && x.to_time == dayEndUtc)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            if (!forceRefresh && existing != null && existing.updated_at >= freshnessCutoff)
            {
                var stored = LiveDayAggregate.FromEntity(existing);
                await SetCacheAsync(cacheKey, stored, TimeSpan.FromMinutes(55));
                return stored;
            }

            var lockKey = $"{businessId}:{tenantId?.ToString() ?? "business"}:{dayStartUtc:yyyyMMdd}";
            var gate = LiveAggregateLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                existing = await db.tbl_dashboard_aggregate
                    .Where(x => x.fk_business == businessId
                                && x.fk_tenant == tenantId
                                && x.granularity == LiveTodayGranularity
                                && x.from_time == dayStartUtc
                                && x.to_time == dayEndUtc)
                    .OrderByDescending(x => x.updated_at)
                    .FirstOrDefaultAsync();

                if (!forceRefresh && existing != null && existing.updated_at >= freshnessCutoff)
                {
                    var stored = LiveDayAggregate.FromEntity(existing);
                    await SetCacheAsync(cacheKey, stored, TimeSpan.FromMinutes(55));
                    return stored;
                }

                var rows = await GetReadingRowsAsync(dayStartUtc, nowUtc, businessId, officeIds);
                var energyKwh = CalculateConsumptionBySensor(rows).Sum(x => x.totalKwh);
                var peakDemandW = CalculateDemandBucketsByMinutes(rows, 15)
                    .DefaultIfEmpty()
                    .Max(x => x?.demandW ?? 0);
                var estimatedCost = tariffConfigured ? energyKwh * tariffRatePerKwh : 0;

                if (existing == null)
                {
                    existing = new tbl_dashboard_aggregate
                    {
                        fk_business = businessId,
                        fk_tenant = tenantId,
                        from_time = dayStartUtc,
                        to_time = dayEndUtc,
                        granularity = LiveTodayGranularity,
                        created_at = nowUtc
                    };
                    db.tbl_dashboard_aggregate.Add(existing);
                }

                existing.total_energy_kwh = Math.Round(energyKwh, 4);
                existing.estimated_cost = Math.Round(estimatedCost, 4);
                existing.peak_demand_w = Math.Round(peakDemandW, 2);
                existing.updated_at = nowUtc;
                await db.SaveChangesAsync();

                var result = LiveDayAggregate.FromEntity(existing);
                await SetCacheAsync(cacheKey, result, TimeSpan.FromMinutes(55));
                return result;
            }
            finally
            {
                gate.Release();
            }
        }

        private List<DemandBucket> CalculateDemandBucketsByMinutes(List<ReadingRow> rows, int minutes)
        {
            var safeMinutes = Math.Max(1, minutes);
            return rows
                .Where(x => x.activePower >= 0 && x.activePower <= 1000000)
                .GroupBy(x => new
                {
                    x.sensorId,
                    bucketStart = new DateTime(
                        x.createdAt.Year,
                        x.createdAt.Month,
                        x.createdAt.Day,
                        x.createdAt.Hour,
                        (x.createdAt.Minute / safeMinutes) * safeMinutes,
                        0,
                        DateTimeKind.Utc)
                })
                .Select(g => new
                {
                    g.Key.bucketStart,
                    sensorAveragePower = g.Average(x => Math.Max(0, x.activePower))
                })
                .GroupBy(x => x.bucketStart)
                .Select(g => new DemandBucket
                {
                    bucketStart = g.Key,
                    demandW = g.Sum(x => x.sensorAveragePower)
                })
                .ToList();
        }

        private async Task<CrmDashboardSummaryResponseDTO> BuildAndStoreSummaryAsync(Guid businessId, Guid? tenantId, List<Guid>? officeIds, DashboardRange range)
        {
            var cacheKey = tenantId.HasValue
                ? $"dashboard:crm:tenant:{tenantId}:{businessId}:summary:30d"
                : $"dashboard:crm:business:{businessId}:summary:30d";

            var cached = await GetCacheAsync<CrmDashboardSummaryResponseDTO>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var rows = await GetReadingRowsAsync(range.fromDate, range.toDate, businessId, officeIds);
            var setting = await GetDashboardSettingAsync(businessId);
            var latestBySensor = rows
                .GroupBy(x => x.sensorId)
                .Select(g => g.OrderByDescending(x => x.createdAt).First())
                .ToList();

            var totalEnergy = CalculateConsumptionBySensor(rows).Sum(x => x.totalKwh);
            var currentLoad = latestBySensor.Sum(x => Math.Max(0, x.activePower));
            var onlineThreshold = DateTime.UtcNow.AddSeconds(-setting.online_sensor_threshold_seconds);
            var onlineSensors = latestBySensor.Count(x => x.createdAt >= onlineThreshold);
            var peakDemand = CalculateDemandBuckets(rows, "hour").DefaultIfEmpty().Max(x => x?.demandW ?? 0);
            var savingOpportunity = totalEnergy * 0.08;
            var estimatedCost = totalEnergy * (double)setting.tariff_rate;

            var result = new CrmDashboardSummaryResponseDTO
            {
                totalEnergyKwh = Math.Round(totalEnergy, 2),
                currentLoadW = Math.Round(currentLoad, 2),
                monthlyCost = Math.Round(estimatedCost, 2),
                onlineSensors = onlineSensors,
                savingOpportunity = Math.Round(savingOpportunity, 2),
                peakDemandW = Math.Round(peakDemand, 2),
                fromDate = range.fromDate.ToString("o"),
                toDate = range.toDate.ToString("o")
            };

            var existing = await db.tbl_dashboard_aggregate
                .FirstOrDefaultAsync(x => x.fk_business == businessId && x.fk_tenant == tenantId && x.from_time == range.fromDate && x.to_time == range.toDate && x.granularity == "30d");

            if (existing == null)
            {
                db.tbl_dashboard_aggregate.Add(new tbl_dashboard_aggregate
                {
                    fk_business = businessId,
                    fk_tenant = tenantId,
                    from_time = range.fromDate,
                    to_time = range.toDate,
                    granularity = "30d",
                    total_energy_kwh = result.totalEnergyKwh,
                    current_load_w = result.currentLoadW,
                    estimated_cost = result.monthlyCost,
                    online_sensors = result.onlineSensors,
                    saving_opportunity = result.savingOpportunity,
                    peak_demand_w = result.peakDemandW
                });
            }
            else
            {
                existing.total_energy_kwh = result.totalEnergyKwh;
                existing.current_load_w = result.currentLoadW;
                existing.estimated_cost = result.monthlyCost;
                existing.online_sensors = result.onlineSensors;
                existing.saving_opportunity = result.savingOpportunity;
                existing.peak_demand_w = result.peakDemandW;
                existing.updated_at = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            await SetCacheAsync(cacheKey, result);
            await CleanupOldDashboardAggregatesAsync();
            return result;
        }

        private async Task<CrmDashboardChartResponseDTO> BuildAndStoreChartAsync(Guid businessId, Guid? tenantId, List<Guid>? officeIds, string chartType, DashboardRange range)
        {
            chartType = NormalizeChartType(chartType);
            var cacheKey = BuildCrmChartCacheKey(businessId, tenantId, chartType, range);

            var cached = await GetCacheAsync<CrmDashboardChartResponseDTO>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            if (range.rangeKey == "custom" && IsOneYearCacheBackedChart(chartType) && IsInsideRollingOneYear(range))
            {
                var oneYearChart = await BuildAndStoreChartAsync(businessId, tenantId, officeIds, chartType, ResolveDashboardRange("1y", null, null));
                var filteredChart = FilterChartFromCachedOneYear(oneYearChart, range);
                await SetCacheAsync(cacheKey, filteredChart);
                return filteredChart;
            }

            var result = await BuildChartFromDbAsync(businessId, tenantId, officeIds, chartType, range);
            await SetCacheAsync(cacheKey, result);
            return result;
        }

        private async Task<CrmDashboardChartResponseDTO> BuildChartFromDbAsync(Guid businessId, Guid? tenantId, List<Guid>? officeIds, string chartType, DashboardRange range)
        {
            var rows = await GetReadingRowsAsync(range.fromDate, range.toDate, businessId, officeIds);
            var setting = await GetDashboardSettingAsync(businessId);
            CrmDashboardChartResponseDTO result = chartType switch
            {
                "peaknonpeak" => BuildPeakNonPeakChart(rows, setting, range),
                "peakdemand" => BuildPeakDemandChart(rows, range),
                "highdemand" => BuildPeakDemandChart(rows, range),
                "hourlyusage" => BuildHourlyUsageChart(rows, range),
                "utilitywise" => BuildUtilityWiseChart(rows, range),
                _ => BuildEnergyConsumptionChart(rows, range)
            };

            result.chartType = chartType;
            result.range = range.rangeKey;
            result.fromDate = range.fromDate.ToString("o");
            result.toDate = range.toDate.ToString("o");
            result.peakStartTime = setting.peak_start_time;
            result.peakEndTime = setting.peak_end_time;

            await UpsertChartAggregateAsync(businessId, tenantId, chartType, range, result);
            return result;
        }

        private async Task UpsertChartAggregateAsync(Guid businessId, Guid? tenantId, string chartType, DashboardRange range, CrmDashboardChartResponseDTO result)
        {
            var payload = JsonSerializer.Serialize(result);
            var existing = await db.tbl_dashboard_chart_aggregate
                .FirstOrDefaultAsync(x => x.fk_business == businessId && x.fk_tenant == tenantId && x.chart_type == chartType && x.range_key == range.rangeKey && x.from_time == range.fromDate && x.to_time == range.toDate);

            if (existing == null)
            {
                db.tbl_dashboard_chart_aggregate.Add(new tbl_dashboard_chart_aggregate
                {
                    fk_business = businessId,
                    fk_tenant = tenantId,
                    chart_type = chartType,
                    range_key = range.rangeKey,
                    from_time = range.fromDate,
                    to_time = range.toDate,
                    payload_json = payload
                });
            }
            else
            {
                existing.payload_json = payload;
                existing.updated_at = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
        }

        private async Task<List<CrmDashboardSuggestionResponseDTO>> ReadActiveSuggestionsAsync(
            Guid businessId,
            Guid? tenantId,
            List<Guid>? officeIds)
        {
            var now = DateTime.UtcNow;
            var query = db.tbl_dashboard_suggestion
                .AsNoTracking()
                .Where(x => x.fk_business == businessId
                    && x.reason_code.StartsWith("LIVE_")
                    && x.to_time > now);

            // Optimization Worker stores business-level suggestions once. Tenant users may see
            // business-level items plus suggestions for offices assigned to their agreement.
            if (tenantId.HasValue)
            {
                var allowedOfficeIds = officeIds ?? new List<Guid>();
                query = query.Where(x =>
                    x.fk_office == null
                    || allowedOfficeIds.Contains(x.fk_office.Value));
            }

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
                .OrderByDescending(x => SeverityRank(x.severity))
                .ThenByDescending(x => x.updated_at)
                .Take(30)
                .Select(x => new CrmDashboardSuggestionResponseDTO
                {
                    suggestionId = x.dashboard_suggestion_id.ToString(),
                    severity = x.severity,
                    type = x.type,
                    title = x.title,
                    message = x.description,
                    action = string.IsNullOrWhiteSpace(x.action) ? x.recommendation : x.action,
                    estimatedSavingKwh = x.estimated_saving_kwh,
                    estimatedSavingCost = x.estimated_saving_cost,
                    sensorId = x.fk_sensor?.ToString() ?? string.Empty,
                    applianceId = x.fk_appliance?.ToString() ?? string.Empty,
                    applianceName = x.affected_appliance,
                    utilityName = x.affected_utility,
                    officeName = x.affected_office,
                    timeBucket = x.updated_at.ToString("O"),
                    canApplyAction = x.can_apply_action,
                    conflictsWithPeakHour = x.conflicts_with_peak_hour,
                    reasonCode = x.reason_code
                })
                .ToList();
        }

        private async Task<List<CrmDashboardSuggestionResponseDTO>> BuildAndStoreSuggestionsAsync(Guid businessId, Guid? tenantId, List<Guid>? officeIds, DashboardRange range)
        {
            var cacheKey = tenantId.HasValue
                ? $"dashboard:crm:tenant:{tenantId}:{businessId}:suggestions:7d"
                : $"dashboard:crm:business:{businessId}:suggestions:7d";

            var cached = await GetCacheAsync<List<CrmDashboardSuggestionResponseDTO>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var rows = await GetReadingRowsAsync(range.fromDate, range.toDate, businessId, officeIds);
            var setting = await GetDashboardSettingAsync(businessId);
            var suggestions = new List<CrmDashboardSuggestionResponseDTO>();

            if (rows.Count < 10)
            {
                suggestions.Add(new CrmDashboardSuggestionResponseDTO
                {
                    severity = "info",
                    type = "metadata",
                    title = "More readings needed",
                    message = "Not enough recent samples are available to produce accurate optimization suggestions.",
                    action = "Keep sensors online and review again after more data is collected.",
                    reasonCode = "INSUFFICIENT_SAMPLES"
                });
            }

            var demandBuckets = CalculateDemandBuckets(rows, "hour");
            if (demandBuckets.Count > 0)
            {
                var averageDemand = demandBuckets.Average(x => x.demandW);
                var threshold = Math.Max(1000, averageDemand * 1.25);
                var conflictBuckets = demandBuckets
                    .Where(x => x.demandW >= threshold && IsPeakLocal(ToPakistanLocal(x.bucketStart), setting))
                    .OrderByDescending(x => x.demandW)
                    .Take(3)
                    .ToList();

                foreach (var bucket in conflictBuckets)
                {
                    var contributors = await GetTopContributorsAsync(bucket.bucketStart, bucket.bucketStart.AddHours(1), businessId, officeIds);
                    var optimizationCandidates = contributors
                        .Where(x => x.isOptimizationCandidate)
                        .Select(x => x.applianceName)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .Take(4)
                        .ToList();

                    suggestions.Add(new CrmDashboardSuggestionResponseDTO
                    {
                        severity = "warning",
                        type = "peak-demand-conflict",
                        title = "High demand overlaps peak hour",
                        message = $"Demand reached {Math.Round(bucket.demandW, 0)} W during the configured peak window ({setting.peak_start_time}-{setting.peak_end_time}).",
                        action = optimizationCandidates.Count > 0
                            ? $"Review these non-critical Normal/Low-priority appliances for manual load reduction or rescheduling: {string.Join(", ", optimizationCandidates)}."
                            : "No safe optimization candidates were found from the available appliance priority, criticality, and electrical profile data.",
                        estimatedSavingKwh = null,
                        estimatedSavingCost = null,
                        applianceName = optimizationCandidates.FirstOrDefault() ?? string.Empty,
                        utilityName = contributors.FirstOrDefault()?.utilityName ?? string.Empty,
                        officeName = contributors.FirstOrDefault()?.officeName ?? string.Empty,
                        timeBucket = ToPakistanLocal(bucket.bucketStart).ToString("yyyy-MM-dd HH:00"),
                        canApplyAction = false,
                        conflictsWithPeakHour = true,
                        reasonCode = optimizationCandidates.Count > 0
                            ? "HIGH_DEMAND_PEAK_OPTIMIZATION_CANDIDATE"
                            : "HIGH_DEMAND_PEAK_PROFILE_MISSING"
                    });
                }
            }

            var metadataMissingCount = await GetAssignedApplianceMetadataMissingCountAsync(businessId, officeIds);
            if (metadataMissingCount > 0)
            {
                suggestions.Add(new CrmDashboardSuggestionResponseDTO
                {
                    severity = "info",
                    type = "metadata",
                    title = "Appliance optimization settings missing",
                    message = $"{metadataMissingCount} assigned appliance(s) have incomplete optimization settings.",
                    action = "Complete the rated voltage, current range, power range, standby power, normal power factor, priority level, and critical-device classification.",
                    reasonCode = "APPLIANCE_METADATA_MISSING"
                });
            }

            suggestions = suggestions
                .GroupBy(x => new { x.reasonCode, x.timeBucket, x.title })
                .Select(g => g.First())
                .Take(10)
                .ToList();

            var oldRows = await db.tbl_dashboard_suggestion
                .Where(x => x.fk_business == businessId && x.fk_tenant == tenantId && x.from_time == range.fromDate && x.to_time == range.toDate)
                .ToListAsync();
            db.tbl_dashboard_suggestion.RemoveRange(oldRows);

            foreach (var suggestion in suggestions)
            {
                var entity = new tbl_dashboard_suggestion
                {
                    fk_business = businessId,
                    fk_tenant = tenantId,
                    from_time = range.fromDate,
                    to_time = range.toDate,
                    type = suggestion.type,
                    severity = suggestion.severity,
                    title = suggestion.title,
                    description = suggestion.message,
                    action = suggestion.action,
                    estimated_saving_kwh = suggestion.estimatedSavingKwh,
                    estimated_saving_cost = suggestion.estimatedSavingCost,
                    affected_appliance = suggestion.applianceName,
                    affected_utility = suggestion.utilityName,
                    affected_office = suggestion.officeName,
                    recommendation = suggestion.action,
                    reason = suggestion.message,
                    reason_code = suggestion.reasonCode,
                    conflicts_with_peak_hour = suggestion.conflictsWithPeakHour,
                    can_apply_action = suggestion.canApplyAction,
                    priority = suggestion.severity
                };
                db.tbl_dashboard_suggestion.Add(entity);
                suggestion.suggestionId = entity.dashboard_suggestion_id.ToString();
            }

            await db.SaveChangesAsync();
            await SetCacheAsync(cacheKey, suggestions);
            return suggestions;
        }

        private CrmDashboardChartResponseDTO BuildEnergyConsumptionChart(List<ReadingRow> rows, DashboardRange range)
        {
            var bucketMode = range.rangeKey == "24h" ? "hour" : "day";
            var totals = CalculateConsumptionBySensor(rows, bucketMode)
                .GroupBy(x => x.bucketStart)
                .Select(g => new { bucket = g.Key, total = g.Sum(x => x.totalKwh) })
                .OrderBy(x => x.bucket)
                .ToList();

            return new CrmDashboardChartResponseDTO
            {
                unit = "kWh",
                categories = totals.Select(x => FormatBucketLabel(x.bucket, range.rangeKey)).ToList(),
                series = new List<CrmDashboardChartSeriesDTO>
                {
                    new() { name = "Energy Consumption", data = totals.Select(x => Math.Round(x.total, 2)).ToList() }
                },
                points = totals.Select(x => new CrmDashboardChartPointDTO
                {
                    label = FormatBucketLabel(x.bucket, range.rangeKey),
                    period = x.bucket.ToString("o"),
                    value = Math.Round(x.total, 2),
                    totalKwh = Math.Round(x.total, 2)
                }).ToList(),
                totalKwh = Math.Round(totals.Sum(x => x.total), 2)
            };
        }

        private CrmDashboardChartResponseDTO BuildHourlyUsageChart(List<ReadingRow> rows, DashboardRange range)
        {
            var totals = CalculateConsumptionBySensor(rows, "hour")
                .GroupBy(x => x.bucketStart)
                .Select(g => new { bucket = g.Key, total = g.Sum(x => x.totalKwh) })
                .OrderBy(x => x.bucket)
                .ToList();

            return new CrmDashboardChartResponseDTO
            {
                unit = "kWh",
                categories = totals.Select(x => FormatBucketLabel(x.bucket, "24h")).ToList(),
                series = new List<CrmDashboardChartSeriesDTO>
                {
                    new() { name = "Hourly Usage", data = totals.Select(x => Math.Round(x.total, 2)).ToList() }
                },
                points = totals.Select(x => new CrmDashboardChartPointDTO
                {
                    label = FormatBucketLabel(x.bucket, "24h"),
                    period = x.bucket.ToString("o"),
                    value = Math.Round(x.total, 2),
                    totalKwh = Math.Round(x.total, 2)
                }).ToList(),
                totalKwh = Math.Round(totals.Sum(x => x.total), 2)
            };
        }

        private CrmDashboardChartResponseDTO BuildUtilityWiseChart(List<ReadingRow> rows, DashboardRange range)
        {
            var totals = CalculateConsumptionBySensor(rows)
                .GroupBy(x => x.utilityName)
                .Select(g => new { utility = g.Key, total = g.Sum(x => x.totalKwh) })
                .OrderByDescending(x => x.total)
                .ToList();

            return new CrmDashboardChartResponseDTO
            {
                unit = "kWh",
                categories = totals.Select(x => x.utility).ToList(),
                series = new List<CrmDashboardChartSeriesDTO>
                {
                    new() { name = "Utility-wise Usage", data = totals.Select(x => Math.Round(x.total, 2)).ToList() }
                },
                points = totals.Select(x => new CrmDashboardChartPointDTO
                {
                    label = x.utility,
                    value = Math.Round(x.total, 2),
                    totalKwh = Math.Round(x.total, 2)
                }).ToList(),
                totalKwh = Math.Round(totals.Sum(x => x.total), 2)
            };
        }

        private CrmDashboardChartResponseDTO BuildPeakNonPeakChart(List<ReadingRow> rows, tbl_business_dashboard_setting setting, DashboardRange range)
        {
            var bucketMode = range.rangeKey == "24h" ? "hour" : "day";
            var totals = new Dictionary<DateTime, (double peak, double nonPeak)>();

            foreach (var item in CalculateIntervalConsumption(rows))
            {
                var bucket = BuildBucketStart(item.currentCreatedAt, bucketMode);
                var isPeak = IsPeakLocal(ToPakistanLocal(item.currentCreatedAt), setting);
                if (!totals.ContainsKey(bucket))
                {
                    totals[bucket] = (0, 0);
                }

                var value = totals[bucket];
                if (isPeak)
                {
                    value.peak += item.consumptionKwh;
                }
                else
                {
                    value.nonPeak += item.consumptionKwh;
                }
                totals[bucket] = value;
            }

            var ordered = totals.OrderBy(x => x.Key).ToList();
            var peak = ordered.Select(x => Math.Round(x.Value.peak, 2)).ToList();
            var nonPeak = ordered.Select(x => Math.Round(x.Value.nonPeak, 2)).ToList();

            return new CrmDashboardChartResponseDTO
            {
                unit = "kWh",
                categories = ordered.Select(x => FormatBucketLabel(x.Key, range.rangeKey)).ToList(),
                series = new List<CrmDashboardChartSeriesDTO>
                {
                    new() { name = "Peak kWh", data = peak },
                    new() { name = "Non-Peak kWh", data = nonPeak }
                },
                points = ordered.Select(x => new CrmDashboardChartPointDTO
                {
                    label = FormatBucketLabel(x.Key, range.rangeKey),
                    period = x.Key.ToString("o"),
                    peakKwh = Math.Round(x.Value.peak, 2),
                    nonPeakKwh = Math.Round(x.Value.nonPeak, 2),
                    totalKwh = Math.Round(x.Value.peak + x.Value.nonPeak, 2),
                    value = Math.Round(x.Value.peak + x.Value.nonPeak, 2)
                }).ToList(),
                totalPeakKwh = Math.Round(peak.Sum(), 2),
                totalNonPeakKwh = Math.Round(nonPeak.Sum(), 2),
                totalKwh = Math.Round(peak.Sum() + nonPeak.Sum(), 2)
            };
        }

        private CrmDashboardChartResponseDTO BuildPeakDemandChart(List<ReadingRow> rows, DashboardRange range)
        {
            var bucketMode = range.rangeKey == "24h" ? "hour" : "day";
            var totals = CalculateDemandBuckets(rows, bucketMode).OrderBy(x => x.bucketStart).ToList();
            return new CrmDashboardChartResponseDTO
            {
                unit = "W",
                categories = totals.Select(x => FormatBucketLabel(x.bucketStart, range.rangeKey)).ToList(),
                series = new List<CrmDashboardChartSeriesDTO>
                {
                    new() { name = "Peak Demand", data = totals.Select(x => Math.Round(x.demandW, 2)).ToList() }
                },
                points = totals.Select(x => new CrmDashboardChartPointDTO
                {
                    label = FormatBucketLabel(x.bucketStart, range.rangeKey),
                    period = x.bucketStart.ToString("o"),
                    demandW = Math.Round(x.demandW, 2),
                    value = Math.Round(x.demandW, 2)
                }).ToList(),
                peakDemandW = totals.Count > 0 ? Math.Round(totals.Max(x => x.demandW), 2) : 0
            };
        }

        private CrmDashboardChartResponseDTO FilterChartFromCachedOneYear(CrmDashboardChartResponseDTO source, DashboardRange range)
        {
            var result = JsonSerializer.Deserialize<CrmDashboardChartResponseDTO>(JsonSerializer.Serialize(source)) ?? source;
            var originalPoints = result.points ?? new List<CrmDashboardChartPointDTO>();
            var selectedIndexes = new List<int>();

            for (var i = 0; i < originalPoints.Count; i++)
            {
                var point = originalPoints[i];
                if (!DateTime.TryParse(point.period, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var pointDate))
                {
                    continue;
                }

                pointDate = EnsureUtc(pointDate);
                if (pointDate >= range.fromDate && pointDate <= range.toDate)
                {
                    selectedIndexes.Add(i);
                }
            }

            result.points = selectedIndexes.Select(index => originalPoints[index]).ToList();
            result.categories = result.points.Select(point => point.label).ToList();

            if (result.series != null)
            {
                foreach (var series in result.series)
                {
                    if (series.data == null || series.data.Count != originalPoints.Count)
                    {
                        continue;
                    }

                    series.data = selectedIndexes.Select(index => series.data[index]).ToList();
                }
            }

            result.range = "custom";
            result.fromDate = range.fromDate.ToString("o");
            result.toDate = range.toDate.ToString("o");
            RecalculateChartTotals(result);
            return result;
        }

        private static void RecalculateChartTotals(CrmDashboardChartResponseDTO chart)
        {
            var points = chart.points ?? new List<CrmDashboardChartPointDTO>();
            chart.totalKwh = Math.Round(points.Sum(point => point.totalKwh), 2);
            chart.totalPeakKwh = Math.Round(points.Sum(point => point.peakKwh), 2);
            chart.totalNonPeakKwh = Math.Round(points.Sum(point => point.nonPeakKwh), 2);
            chart.peakDemandW = points.Count > 0 ? Math.Round(points.Max(point => point.demandW), 2) : 0;
        }

        private static bool IsInsideRollingOneYear(DashboardRange range)
        {
            var now = DateTime.UtcNow;
            var oneYearAgo = now.AddYears(-1);
            var fromDate = EnsureUtc(range.fromDate);
            var toDate = EnsureUtc(range.toDate);

            return fromDate >= oneYearAgo && toDate <= now.AddMinutes(5) && fromDate < toDate;
        }

        private static bool IsOneYearCacheBackedChart(string chartType)
        {
            return chartType is "energyconsumption" or "peaknonpeak" or "peakdemand" or "highdemand";
        }


        private async Task<List<ReadingRow>> GetReadingRowsAsync(DateTime fromDate, DateTime toDate, Guid? businessId, List<Guid>? officeIds)
        {
            var query =
                from reading in db.tbl_singal_phase_data.AsNoTracking()
                join sensor in db.tbl_sensor.AsNoTracking() on reading.fk_sensor equals sensor.sensor_id
                join device in db.tbl_device.AsNoTracking() on sensor.fk_device equals device.device_id
                join utility in db.tbl_utility.AsNoTracking() on sensor.fk_utility equals utility.utility_id into utilityJoin
                from utility in utilityJoin.DefaultIfEmpty()
                join office in db.tbl_office.AsNoTracking() on device.fk_office equals office.office_id into officeJoin
                from office in officeJoin.DefaultIfEmpty()
                where !reading.is_deleted
                      && !sensor.is_deleted
                      && !device.is_deleted
                      && reading.created_at >= fromDate
                      && reading.created_at <= toDate
                      && (!businessId.HasValue || device.fk_business == businessId.Value)
                select new ReadingRow
                {
                    sensorId = sensor.sensor_id,
                    sensorName = sensor.sensor_name,
                    businessId = device.fk_business,
                    officeId = device.fk_office,
                    officeName = office != null ? office.office_name : string.Empty,
                    utilityName = utility != null ? utility.utility_name : "Unknown",
                    createdAt = reading.created_at,
                    activeEnergy = reading.active_energy,
                    activePower = reading.active_power,
                    current = reading.current,
                    powerFactor = reading.power_factor
                };

            var rows = await query.ToListAsync();
            if (officeIds != null)
            {
                var allowed = officeIds.ToHashSet();
                rows = rows.Where(x => allowed.Contains(x.officeId)).ToList();
            }
            return rows;
        }

        private List<ConsumptionBySensorBucket> CalculateConsumptionBySensor(List<ReadingRow> rows, string bucketMode = "all")
        {
            return CalculateIntervalConsumption(rows)
                .GroupBy(x => new
                {
                    x.sensorId,
                    x.utilityName,
                    bucketStart = bucketMode == "all" ? new DateTime(2000, 1, 1) : BuildBucketStart(x.currentCreatedAt, bucketMode)
                })
                .Select(g => new ConsumptionBySensorBucket
                {
                    sensorId = g.Key.sensorId,
                    utilityName = g.Key.utilityName,
                    bucketStart = g.Key.bucketStart,
                    totalKwh = g.Sum(x => x.consumptionKwh)
                })
                .ToList();
        }

        private List<IntervalConsumption> CalculateIntervalConsumption(List<ReadingRow> rows)
        {
            var result = new List<IntervalConsumption>();
            foreach (var sensorGroup in rows.GroupBy(x => x.sensorId))
            {
                var ordered = sensorGroup.OrderBy(x => x.createdAt).ToList();
                if (ordered.Count < 2)
                {
                    continue;
                }

                for (var i = 1; i < ordered.Count; i++)
                {
                    var previous = ordered[i - 1];
                    var current = ordered[i];
                    var diff = current.activeEnergy - previous.activeEnergy;

                    if (diff <= 0 || diff > 1000)
                    {
                        continue;
                    }

                    result.Add(new IntervalConsumption
                    {
                        sensorId = current.sensorId,
                        utilityName = current.utilityName,
                        currentCreatedAt = current.createdAt,
                        consumptionKwh = diff
                    });
                }
            }
            return result;
        }

        private List<DemandBucket> CalculateDemandBuckets(List<ReadingRow> rows, string bucketMode)
        {
            return rows
                .Where(x => x.activePower >= 0 && x.activePower <= 1000000)
                .GroupBy(x => new { x.sensorId, bucketStart = BuildBucketStart(x.createdAt, bucketMode) })
                .Select(g => new { g.Key.bucketStart, sensorAveragePower = g.Average(x => Math.Max(0, x.activePower)) })
                .GroupBy(x => x.bucketStart)
                .Select(g => new DemandBucket { bucketStart = g.Key, demandW = g.Sum(x => x.sensorAveragePower) })
                .ToList();
        }

        private async Task<List<ContributorRow>> GetTopContributorsAsync(DateTime fromDate, DateTime toDate, Guid businessId, List<Guid>? officeIds)
        {
            var query =
                from reading in db.tbl_singal_phase_data.AsNoTracking()
                join sensor in db.tbl_sensor.AsNoTracking() on reading.fk_sensor equals sensor.sensor_id
                join device in db.tbl_device.AsNoTracking() on sensor.fk_device equals device.device_id
                join utility in db.tbl_utility.AsNoTracking() on sensor.fk_utility equals utility.utility_id into utilityJoin
                from utility in utilityJoin.DefaultIfEmpty()
                join office in db.tbl_office.AsNoTracking() on device.fk_office equals office.office_id into officeJoin
                from office in officeJoin.DefaultIfEmpty()
                join assignment in db.tbl_sensor_appliance.AsNoTracking().Where(x => x.is_active && !x.is_deleted) on sensor.sensor_id equals assignment.fk_sensor into assignmentJoin
                from assignment in assignmentJoin.DefaultIfEmpty()
                join appliance in db.tbl_business_appliance.AsNoTracking().Where(x => !x.is_deleted) on assignment.fk_appliance equals appliance.business_appliance_id into applianceJoin
                from appliance in applianceJoin.DefaultIfEmpty()
                where !reading.is_deleted
                      && !sensor.is_deleted
                      && !device.is_deleted
                      && device.fk_business == businessId
                      && reading.created_at >= fromDate
                      && reading.created_at < toDate
                select new ContributorRow
                {
                    sensorId = sensor.sensor_id,
                    sensorName = sensor.sensor_name,
                    officeId = device.fk_office,
                    officeName = office != null ? office.office_name : string.Empty,
                    utilityName = utility != null ? utility.utility_name : "Unknown",
                    applianceId = appliance != null ? appliance.business_appliance_id : Guid.Empty,
                    applianceName = appliance != null ? appliance.appliance_name : string.Empty,
                    priorityLevel = appliance != null ? appliance.priority_level : string.Empty,
                    isCritical = appliance != null && appliance.is_critical,
                    ratedVoltage = appliance != null ? appliance.rated_voltage : 0,
                    minCurrent = appliance != null ? appliance.min_current : 0,
                    maxCurrent = appliance != null ? appliance.max_current : 0,
                    minPower = appliance != null ? appliance.min_power : 0,
                    maxPower = appliance != null ? appliance.max_power : 0,
                    standbyPower = appliance != null ? appliance.standby_power : 0,
                    normalPowerFactor = appliance != null ? appliance.normal_power_factor : 0,
                    powerW = reading.active_power
                };

            var rows = await query.ToListAsync();
            if (officeIds != null)
            {
                var allowed = officeIds.ToHashSet();
                rows = rows.Where(x => allowed.Contains(x.officeId)).ToList();
            }

            foreach (var row in rows)
            {
                row.isOptimizationCandidate = row.applianceId != Guid.Empty
                    && IsSafeOptimizationCandidate(
                        row.priorityLevel,
                        row.isCritical,
                        row.ratedVoltage,
                        row.minCurrent,
                        row.maxCurrent,
                        row.minPower,
                        row.maxPower,
                        row.standbyPower,
                        row.normalPowerFactor);
            }

            return rows
                .GroupBy(x => new
                {
                    x.sensorId,
                    x.sensorName,
                    x.officeName,
                    x.utilityName,
                    x.applianceId,
                    x.applianceName,
                    x.isOptimizationCandidate
                })
                .Select(g => new ContributorRow
                {
                    sensorId = g.Key.sensorId,
                    sensorName = g.Key.sensorName,
                    officeId = g.First().officeId,
                    officeName = g.Key.officeName,
                    utilityName = g.Key.utilityName,
                    applianceId = g.Key.applianceId,
                    applianceName = g.Key.applianceName,
                    isOptimizationCandidate = g.Key.isOptimizationCandidate,
                    powerW = g.Average(x => Math.Max(0, x.powerW))
                })
                .OrderByDescending(x => x.powerW)
                .Take(8)
                .ToList();
        }

        private async Task<int> GetAssignedApplianceMetadataMissingCountAsync(Guid businessId, List<Guid>? officeIds)
        {
            var query =
                from assignment in db.tbl_sensor_appliance.AsNoTracking()
                join sensor in db.tbl_sensor.AsNoTracking() on assignment.fk_sensor equals sensor.sensor_id
                join device in db.tbl_device.AsNoTracking() on sensor.fk_device equals device.device_id
                join appliance in db.tbl_business_appliance.AsNoTracking() on assignment.fk_appliance equals appliance.business_appliance_id
                where !assignment.is_deleted
                      && assignment.is_active
                      && !sensor.is_deleted
                      && !device.is_deleted
                      && !appliance.is_deleted
                      && appliance.is_active
                      && device.fk_business == businessId
                select new
                {
                    device.fk_office,
                    appliance.priority_level,
                    appliance.rated_voltage,
                    appliance.min_current,
                    appliance.max_current,
                    appliance.min_power,
                    appliance.max_power,
                    appliance.standby_power,
                    appliance.normal_power_factor
                };

            var rows = await query.ToListAsync();
            if (officeIds != null)
            {
                var allowed = officeIds.ToHashSet();
                rows = rows.Where(x => allowed.Contains(x.fk_office)).ToList();
            }

            return rows.Count(x => !HasCompleteOptimizationProfile(
                x.priority_level,
                x.rated_voltage,
                x.min_current,
                x.max_current,
                x.min_power,
                x.max_power,
                x.standby_power,
                x.normal_power_factor));
        }

        private async Task<List<Guid>> GetTenantOfficeIdsAsync(Guid tenantId, Guid? businessId)
        {
            var query =
                from agreement in db.tbl_agreement.AsNoTracking()
                join officeAgreement in db.tbl_office_agreement.AsNoTracking() on agreement.agreement_id equals officeAgreement.fk_agreement
                where !agreement.is_deleted
                      && agreement.is_active
                      && !officeAgreement.is_deleted
                      && agreement.fk_tenant == tenantId
                      && (!businessId.HasValue || agreement.fk_business == businessId.Value)
                select officeAgreement.fk_office;

            return await query.Distinct().ToListAsync();
        }

        private async Task<Guid?> ResolveBusinessIdForTenantAsync(Guid? tenantId, List<Guid>? officeIds)
        {
            if (!tenantId.HasValue)
            {
                return null;
            }

            var businessId = await db.tbl_agreement
                .AsNoTracking()
                .Where(x => x.fk_tenant == tenantId.Value && x.is_active && !x.is_deleted)
                .Select(x => (Guid?)x.fk_business)
                .FirstOrDefaultAsync();

            if (businessId.HasValue)
            {
                return businessId;
            }

            if (officeIds != null && officeIds.Count > 0)
            {
                return await db.tbl_office
                    .AsNoTracking()
                    .Where(x => officeIds.Contains(x.office_id) && !x.is_deleted)
                    .Select(x => (Guid?)x.fk_business)
                    .FirstOrDefaultAsync();
            }

            return null;
        }

        private async Task<tbl_business_dashboard_setting> GetDashboardSettingAsync(Guid? businessId)
        {
            if (businessId.HasValue && businessId.Value != Guid.Empty)
            {
                var existing = await db.tbl_business_dashboard_setting
                    .Where(x => x.fk_business == businessId.Value && x.is_active && !x.is_deleted)
                    .OrderBy(x => x.day_of_week.HasValue)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return existing;
                }

                var created = new tbl_business_dashboard_setting
                {
                    fk_business = businessId.Value,
                    peak_start_time = "18:00",
                    peak_end_time = "23:00",
                    is_active = true
                };
                db.tbl_business_dashboard_setting.Add(created);
                await db.SaveChangesAsync();
                return created;
            }

            return new tbl_business_dashboard_setting();
        }

        private async Task CleanupOldDashboardAggregatesAsync()
        {
            var cutoff = DateTime.UtcNow.AddYears(-3);
            var oldAggregates = await db.tbl_dashboard_aggregate.Where(x => x.to_time < cutoff).ToListAsync();
            var oldCharts = await db.tbl_dashboard_chart_aggregate.Where(x => x.to_time < cutoff).ToListAsync();
            var oldSuggestions = await db.tbl_dashboard_suggestion.Where(x => x.to_time < cutoff).ToListAsync();
            if (oldAggregates.Count == 0 && oldCharts.Count == 0 && oldSuggestions.Count == 0)
            {
                return;
            }

            db.tbl_dashboard_aggregate.RemoveRange(oldAggregates);
            db.tbl_dashboard_chart_aggregate.RemoveRange(oldCharts);
            db.tbl_dashboard_suggestion.RemoveRange(oldSuggestions);
            await db.SaveChangesAsync();
        }

        private DashboardRange ResolveDashboardRange(string? range, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;
            var key = NormalizeRangeKey(range);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value < toDate.Value)
            {
                return new DashboardRange
                {
                    rangeKey = "custom",
                    fromDate = EnsureUtc(fromDate.Value),
                    toDate = EnsureUtc(toDate.Value)
                };
            }

            return key switch
            {
                "24h" => new DashboardRange { rangeKey = "24h", fromDate = now.AddHours(-24), toDate = now },
                "7d" => new DashboardRange { rangeKey = "7d", fromDate = now.AddDays(-7), toDate = now },
                "30d" => new DashboardRange { rangeKey = "30d", fromDate = now.AddDays(-30), toDate = now },
                "1y" => new DashboardRange { rangeKey = "1y", fromDate = now.AddYears(-1), toDate = now },
                _ => new DashboardRange { rangeKey = "30d", fromDate = now.AddDays(-30), toDate = now }
            };
        }

        private static string NormalizeRangeKey(string? range)
        {
            var value = (range ?? "30d").Trim().ToLowerInvariant();
            return value switch
            {
                "24h" => "24h",
                "7d" => "7d",
                "30d" => "30d",
                "1y" or "12m" or "365d" or "year" or "lastyear" => "1y",
                "custom" => "custom",
                _ => "30d"
            };
        }

        private static string NormalizeChartType(string? chartType)
        {
            return (chartType ?? "energyconsumption")
                .Trim()
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant() switch
            {
                "peaknonpeak" => "peaknonpeak",
                "peakdemand" => "peakdemand",
                "highdemand" => "highdemand",
                "hourlyusage" => "hourlyusage",
                "hourly" => "hourlyusage",
                "utilitywise" => "utilitywise",
                "utilitywiseusage" => "utilitywise",
                _ => "energyconsumption"
            };
        }

        private static DateTime BuildBucketStart(DateTime utcDate, string bucketMode)
        {
            return bucketMode switch
            {
                "hour" => new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, utcDate.Hour, 0, 0, DateTimeKind.Utc),
                "month" => new DateTime(utcDate.Year, utcDate.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => utcDate.Date
            };
        }

        private static string FormatBucketLabel(DateTime utcDate, string range)
        {
            var local = ToPakistanLocal(utcDate);
            return range == "24h"
                ? local.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture)
                : local.ToString("MMM dd", CultureInfo.InvariantCulture);
        }

        private static DateTime ToPakistanLocal(DateTime utcDate) => EnsureUtc(utcDate).AddHours(PakistanUtcOffsetHours);

        private static bool IsPeakLocal(DateTime localTime, tbl_business_dashboard_setting setting)
        {
            if (setting.day_of_week.HasValue && setting.day_of_week.Value != (int)localTime.DayOfWeek)
            {
                return false;
            }

            if (!TimeSpan.TryParse(setting.peak_start_time, out var start))
            {
                start = new TimeSpan(18, 0, 0);
            }
            if (!TimeSpan.TryParse(setting.peak_end_time, out var end))
            {
                end = new TimeSpan(23, 0, 0);
            }

            var now = localTime.TimeOfDay;
            return start <= end
                ? now >= start && now < end
                : now >= start || now < end;
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }
            if (value.Kind == DateTimeKind.Local)
            {
                return value.ToUniversalTime();
            }
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static Guid? TryParseGuid(string? value)
        {
            return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
        }

        private static string BuildCrmChartCacheKey(Guid businessId, Guid? tenantId, string chartType, DashboardRange range)
        {
            var timeKey = range.rangeKey == "custom"
                ? $"{range.fromDate:yyyyMMddHHmmss}:{range.toDate:yyyyMMddHHmmss}"
                : "rolling";

            return tenantId.HasValue
                ? $"dashboard:crm:tenant:{tenantId}:{businessId}:{range.rangeKey}:{timeKey}:chart:{chartType}"
                : $"dashboard:crm:business:{businessId}:{range.rangeKey}:{timeKey}:chart:{chartType}";
        }

        private static string BuildScopedCacheKey(string prefix, Guid? businessId, Guid? tenantId)
        {
            if (tenantId.HasValue)
            {
                return $"{prefix}:tenant:{tenantId.Value}:business:{businessId?.ToString() ?? "all"}";
            }

            return businessId.HasValue
                ? $"{prefix}:business:{businessId.Value}"
                : $"{prefix}:all";
        }

        private async Task<T?> GetCacheAsync<T>(string key)
        {
            if (redis == null)
            {
                return default;
            }

            try
            {
                var value = await redis.StringGetAsync(key);
                return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
            }
            catch
            {
                return default;
            }
        }

        private Task SetCacheAsync<T>(string key, T value)
        {
            return SetCacheAsync(key, value, CacheTtl);
        }

        private async Task SetCacheAsync<T>(string key, T value, TimeSpan ttl)
        {
            if (redis == null)
            {
                return;
            }

            try
            {
                await redis.StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
            }
            catch
            {
                // Redis is only cache. Do not fail dashboard when Redis is down.
            }
        }

        private static ResponseModel<T> Success<T>(T data) => new()
        {
            data = data,
            remarks = "Success",
            success = true
        };

        private static ResponseModel<T> Failure<T>(Exception ex) => new()
        {
            remarks = $"Error: {ex.Message}",
            success = false
        };

        private class LiveScopeSensor
        {
            public Guid sensorId { get; set; }
            public string sensorName { get; set; } = string.Empty;
            public Guid officeId { get; set; }
            public string officeName { get; set; } = string.Empty;
            public string floorName { get; set; } = string.Empty;
            public string utilityName { get; set; } = string.Empty;
            public string applianceName { get; set; } = string.Empty;
            public bool hasApplianceAssignment { get; set; }
            public bool hasOptimizationProfile { get; set; }
        }

        private class LiveDashboardConfiguration
        {
            public bool tariffConfigured { get; set; }
            public double tariffRatePerKwh { get; set; }
            public bool demandLimitConfigured { get; set; }
            public double? demandLimitKw { get; set; }
            public int onlineThresholdSeconds { get; set; } = MinimumOnlineThresholdSeconds;
        }

        private class LiveDayAggregate
        {
            public double energyKwh { get; set; }
            public double estimatedCost { get; set; }
            public double peakDemandW { get; set; }
            public DateTime updatedAtUtc { get; set; }

            public static LiveDayAggregate FromEntity(tbl_dashboard_aggregate entity) => new()
            {
                energyKwh = entity.total_energy_kwh,
                estimatedCost = entity.estimated_cost,
                peakDemandW = entity.peak_demand_w,
                updatedAtUtc = EnsureUtc(entity.updated_at)
            };
        }

        private class DashboardRange
        {
            public string rangeKey { get; set; } = "30d";
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
        }

        private class ReadingRow
        {
            public Guid sensorId { get; set; }
            public string sensorName { get; set; } = string.Empty;
            public Guid businessId { get; set; }
            public Guid officeId { get; set; }
            public string officeName { get; set; } = string.Empty;
            public string utilityName { get; set; } = string.Empty;
            public DateTime createdAt { get; set; }
            public double activeEnergy { get; set; }
            public double activePower { get; set; }
            public double current { get; set; }
            public double powerFactor { get; set; }
        }

        private class IntervalConsumption
        {
            public Guid sensorId { get; set; }
            public string utilityName { get; set; } = string.Empty;
            public DateTime currentCreatedAt { get; set; }
            public double consumptionKwh { get; set; }
        }

        private class ConsumptionBySensorBucket
        {
            public Guid sensorId { get; set; }
            public string utilityName { get; set; } = string.Empty;
            public DateTime bucketStart { get; set; }
            public double totalKwh { get; set; }
        }

        private class DemandBucket
        {
            public DateTime bucketStart { get; set; }
            public double demandW { get; set; }
        }

        private class ContributorRow
        {
            public Guid sensorId { get; set; }
            public string sensorName { get; set; } = string.Empty;
            public Guid officeId { get; set; }
            public string officeName { get; set; } = string.Empty;
            public string utilityName { get; set; } = string.Empty;
            public Guid applianceId { get; set; }
            public string applianceName { get; set; } = string.Empty;
            public string priorityLevel { get; set; } = string.Empty;
            public bool isCritical { get; set; }
            public double ratedVoltage { get; set; }
            public double minCurrent { get; set; }
            public double maxCurrent { get; set; }
            public double minPower { get; set; }
            public double maxPower { get; set; }
            public double standbyPower { get; set; }
            public double normalPowerFactor { get; set; }
            public double powerW { get; set; }
            public bool isOptimizationCandidate { get; set; }
        }
    }
}