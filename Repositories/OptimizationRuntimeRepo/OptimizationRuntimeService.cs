using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OptimizationRuntimeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.OptimizationRuntimeRepo
{
    public interface IOptimizationRuntimeService
    {
        Task<ResponseModel<OptimizationRuntimeConfigurationDTO>> GetConfigurationAsync(Guid businessId);
        Task<ResponseModel<OptimizationSuggestionSyncResultDTO>> SyncSuggestionsAsync(
            OptimizationSuggestionSyncRequestDTO request,
            CancellationToken cancellationToken = default);
    }

    public class OptimizationRuntimeService : IOptimizationRuntimeService
    {
        private const string LiveReasonPrefix = "LIVE_";
        private readonly DBUserManagementContext db;

        public OptimizationRuntimeService(DBUserManagementContext db)
        {
            this.db = db;
        }

        public async Task<ResponseModel<OptimizationRuntimeConfigurationDTO>> GetConfigurationAsync(Guid businessId)
        {
            var businessExists = await db.tbl_business
                .AsNoTracking()
                .AnyAsync(x => x.business_id == businessId && !x.is_deleted);

            if (!businessExists)
                return Fail<OptimizationRuntimeConfigurationDTO>("Business not found.");

            var dashboardSetting = await db.tbl_business_dashboard_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var plan = await db.tbl_energy_tariff_plan
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var demand = await db.tbl_demand_management_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessId && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var result = new OptimizationRuntimeConfigurationDTO
            {
                BusinessId = businessId,
                TimeZone = string.IsNullOrWhiteSpace(dashboardSetting?.timezone)
                    ? "UTC"
                    : dashboardSetting.timezone,
                Currency = plan?.currency ?? dashboardSetting?.currency ?? "PKR",
                StandardRatePerKwh = plan?.standard_rate_per_kwh ?? dashboardSetting?.tariff_rate ?? 0,
                PeakRatePerKwh = plan?.peak_rate_per_kwh ?? dashboardSetting?.peak_tariff_rate ?? 0,
                OffPeakRatePerKwh = plan?.off_peak_rate_per_kwh ?? dashboardSetting?.off_peak_tariff_rate ?? 0,
                HasTariffPlan = plan != null,
                DemandConfigured = demand != null,
                DemandLimitKw = demand?.demand_limit_kw ?? 0,
                WarningThresholdPercent = demand?.warning_threshold_percent ?? 0,
                RecoveryThresholdKw = demand?.recovery_threshold_kw ?? 0,
                DemandIntervalMinutes = demand?.demand_interval_minutes ?? 15,
                StabilizationMinutes = demand?.stabilization_minutes ?? 5,
                EnablePeakHourControl = demand?.enable_peak_hour_control ?? false,
                EnableDemandThresholdControl = demand?.enable_demand_threshold_control ?? false,
                SuggestionOnlyMode = demand?.suggestion_only_mode ?? true,
                GeneratedAtUtc = DateTime.UtcNow
            };

            if (plan != null)
            {
                var periods = await db.tbl_tariff_time_period
                    .AsNoTracking()
                    .Where(x => x.fk_tariff_plan == plan.energy_tariff_plan_id && x.is_active && !x.is_deleted)
                    .OrderBy(x => x.day_of_week)
                    .ThenBy(x => x.start_time)
                    .ToListAsync();

                result.TariffPeriods = periods.Select(x => new OptimizationTariffPeriodDTO
                {
                    PeriodName = x.period_name,
                    PeriodType = x.period_type,
                    StartTime = x.start_time.ToString("HH:mm"),
                    EndTime = x.end_time.ToString("HH:mm"),
                    DayOfWeek = x.day_of_week,
                    SeasonStart = x.season_start.HasValue ? x.season_start.Value.ToString("yyyy-MM-dd") : null,
                    SeasonEnd = x.season_end.HasValue ? x.season_end.Value.ToString("yyyy-MM-dd") : null
                }).ToList();
            }
            else if (dashboardSetting != null)
            {
                result.TariffPeriods.Add(new OptimizationTariffPeriodDTO
                {
                    PeriodName = "Legacy peak window",
                    PeriodType = "Peak",
                    StartTime = dashboardSetting.peak_start_time,
                    EndTime = dashboardSetting.peak_end_time,
                    DayOfWeek = dashboardSetting.day_of_week
                });
            }

            return new ResponseModel<OptimizationRuntimeConfigurationDTO>
            {
                success = true,
                remarks = "Success",
                data = result
            };
        }

        public async Task<ResponseModel<OptimizationSuggestionSyncResultDTO>> SyncSuggestionsAsync(
            OptimizationSuggestionSyncRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (request.BusinessId == Guid.Empty)
                return Fail<OptimizationSuggestionSyncResultDTO>("Business id is required.");

            var businessExists = await db.tbl_business
                .AnyAsync(x => x.business_id == request.BusinessId && !x.is_deleted, cancellationToken);
            if (!businessExists)
                return Fail<OptimizationSuggestionSyncResultDTO>("Business not found.");

            var now = EnsureUtc(request.GeneratedAtUtc == default ? DateTime.UtcNow : request.GeneratedAtUtc);
            var incoming = request.Suggestions
                .Where(x => !string.IsNullOrWhiteSpace(x.ReasonCode))
                .Take(100)
                .GroupBy(x => x.ReasonCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(x => x.Last())
                .ToList();

            var existing = await db.tbl_dashboard_suggestion
                .Where(x => x.fk_business == request.BusinessId && x.reason_code.StartsWith(LiveReasonPrefix))
                .ToListAsync(cancellationToken);

            var byReason = existing
                .GroupBy(x => x.reason_code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.updated_at).First(),
                    StringComparer.OrdinalIgnoreCase);
            var retainedIds = byReason.Values.Select(x => x.dashboard_suggestion_id).ToHashSet();
            var activeReasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new OptimizationSuggestionSyncResultDTO { SyncedAtUtc = now };

            // Older versions did not enforce a live-suggestion uniqueness rule.
            // Expire duplicates safely instead of failing dictionary creation.
            foreach (var duplicate in existing.Where(x => !retainedIds.Contains(x.dashboard_suggestion_id)))
            {
                duplicate.to_time = now;
                duplicate.updated_at = now;
                result.Expired++;
            }

            foreach (var item in incoming)
            {
                var reasonCode = item.ReasonCode.Trim();
                if (!reasonCode.StartsWith(LiveReasonPrefix, StringComparison.OrdinalIgnoreCase))
                    reasonCode = $"{LiveReasonPrefix}{reasonCode}";

                activeReasons.Add(reasonCode);
                if (!byReason.TryGetValue(reasonCode, out var entity))
                {
                    entity = new tbl_dashboard_suggestion
                    {
                        dashboard_suggestion_id = Guid.NewGuid(),
                        fk_business = request.BusinessId,
                        reason_code = reasonCode,
                        created_at = now
                    };
                    await db.tbl_dashboard_suggestion.AddAsync(entity, cancellationToken);
                    byReason[reasonCode] = entity;
                    result.Inserted++;
                }
                else
                {
                    result.Updated++;
                }

                Apply(entity, item, now);
            }

            foreach (var entity in existing.Where(x => !activeReasons.Contains(x.reason_code) && x.to_time > now))
            {
                entity.to_time = now;
                entity.updated_at = now;
                result.Expired++;
            }

            await db.SaveChangesAsync(cancellationToken);

            return new ResponseModel<OptimizationSuggestionSyncResultDTO>
            {
                success = true,
                remarks = "Optimization suggestions synchronized.",
                data = result
            };
        }

        private static void Apply(
            tbl_dashboard_suggestion entity,
            OptimizationSuggestionItemDTO item,
            DateTime now)
        {
            var detectedAt = EnsureUtc(item.DetectedAtUtc == default ? now : item.DetectedAtUtc);
            var expiresAt = EnsureUtc(item.ExpiresAtUtc == default ? now.AddMinutes(2) : item.ExpiresAtUtc);
            if (expiresAt <= detectedAt) expiresAt = detectedAt.AddMinutes(2);

            entity.fk_sensor = item.SensorId;
            entity.fk_office = item.OfficeId;
            entity.fk_appliance = item.ApplianceId;
            entity.from_time = detectedAt;
            entity.to_time = expiresAt;
            entity.type = Limit(item.Type, 80, "general");
            entity.severity = Limit(item.Severity, 30, "info");
            entity.priority = Limit(item.Priority, 30, "Low");
            entity.title = Limit(item.Title, 250, "Energy optimization suggestion");
            entity.description = Limit(item.Description, 2000, string.Empty);
            entity.action = Limit(item.Recommendation, 1000, string.Empty);
            entity.recommendation = Limit(item.Recommendation, 1000, string.Empty);
            entity.reason = Limit(item.Description, 2000, string.Empty);
            entity.affected_appliance = Limit(item.AffectedAppliance, 250, string.Empty);
            entity.affected_utility = Limit(item.AffectedUtility, 150, string.Empty);
            entity.affected_office = Limit(item.AffectedOffice, 250, string.Empty);
            entity.confidence = Limit(item.Confidence, 30, "Medium");
            entity.estimated_saving_kwh = item.EstimatedSavingKwh;
            entity.estimated_saving_cost = item.EstimatedSavingCost;
            entity.conflicts_with_peak_hour = item.ConflictsWithPeakHour;
            entity.can_apply_action = item.CanApplyAction;
            entity.updated_at = now;
        }

        private static string Limit(string? value, int maxLength, string fallback)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return value;
            if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static ResponseModel<T> Fail<T>(string remarks) => new()
        {
            success = false,
            remarks = remarks,
            data = default!
        };
    }
}
