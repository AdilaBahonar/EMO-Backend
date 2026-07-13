using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.EnergyConfigurationDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.DemandManagementRedisRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.EnergyConfigurationRepo
{
    public class EnergyConfigurationService : IEnergyConfigurationService
    {
        private readonly DBUserManagementContext db;
        private readonly IDemandManagementRedisCacheService demandRedisCache;
        private readonly ILogger<EnergyConfigurationService> logger;

        public EnergyConfigurationService(
            DBUserManagementContext db,
            IDemandManagementRedisCacheService demandRedisCache,
            ILogger<EnergyConfigurationService> logger)
        {
            this.db = db;
            this.demandRedisCache = demandRedisCache;
            this.logger = logger;
        }

        public async Task<ResponseModel<EnergyConfigurationDTO>> GetByBusinessId(string businessId)
        {
            if (!Guid.TryParse(businessId, out var businessGuid))
                return Fail("Invalid business id.");

            var businessExists = await db.tbl_business
                .AnyAsync(x => x.business_id == businessGuid && !x.is_deleted);
            if (!businessExists) return Fail("Business not found.");

            // There is now one currently active tariff configuration per business.
            // updated_at replaces the removed effective date fields for deterministic selection.
            var plan = await db.tbl_energy_tariff_plan
                .AsNoTracking()
                .Where(x => x.fk_business == businessGuid && x.is_active && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var demand = await db.tbl_demand_management_setting
                .AsNoTracking()
                .Where(x => x.fk_business == businessGuid && !x.is_deleted)
                .OrderByDescending(x => x.updated_at)
                .FirstOrDefaultAsync();

            var result = new EnergyConfigurationDTO { fkBusiness = businessId };
            result.tariffPlan.fkBusiness = businessId;
            result.demandManagement.fkBusiness = businessId;

            if (plan != null)
            {
                result.tariffPlan = MapPlan(plan);
                result.tariffPlan.timePeriods = await db.tbl_tariff_time_period
                    .AsNoTracking()
                    .Where(x => x.fk_tariff_plan == plan.energy_tariff_plan_id && !x.is_deleted)
                    .OrderBy(x => x.day_of_week)
                    .ThenBy(x => x.start_time)
                    .Select(x => MapPeriod(x))
                    .ToListAsync();
            }
            else
            {
                // Preserve compatibility with businesses that only have the legacy dashboard rates.
                var legacy = await db.tbl_business_dashboard_setting
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.fk_business == businessGuid && x.is_active && !x.is_deleted);
                if (legacy != null)
                {
                    result.tariffPlan.currency = legacy.currency;
                    result.tariffPlan.standardRatePerKwh = legacy.tariff_rate;
                    result.tariffPlan.peakRatePerKwh = legacy.peak_tariff_rate;
                    result.tariffPlan.offPeakRatePerKwh = legacy.off_peak_tariff_rate;
                    result.tariffPlan.timePeriods.Add(new TariffTimePeriodDTO
                    {
                        periodName = "Legacy peak window",
                        periodType = "Peak",
                        startTime = legacy.peak_start_time,
                        endTime = legacy.peak_end_time,
                        dayOfWeek = legacy.day_of_week,
                        isActive = true
                    });
                }
            }

            if (demand != null) result.demandManagement = MapDemand(demand);

            return new ResponseModel<EnergyConfigurationDTO>
            {
                data = result,
                success = true,
                remarks = "Success"
            };
        }

        public async Task<ResponseModel<EnergyConfigurationDTO>> Save(EnergyConfigurationDTO request)
        {
            if (!Guid.TryParse(request.fkBusiness, out var businessGuid))
                return Fail("Invalid business id.");
            if (!await db.tbl_business.AnyAsync(x => x.business_id == businessGuid && !x.is_deleted))
                return Fail("Business not found.");

            var validation = Validate(request);
            if (!string.IsNullOrEmpty(validation)) return Fail(validation);

            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                var plan = await db.tbl_energy_tariff_plan
                    .Where(x => x.fk_business == businessGuid && x.is_active && !x.is_deleted)
                    .OrderByDescending(x => x.updated_at)
                    .FirstOrDefaultAsync();

                if (plan == null)
                {
                    plan = new tbl_energy_tariff_plan { fk_business = businessGuid };
                    await db.tbl_energy_tariff_plan.AddAsync(plan);
                }

                ApplyPlan(plan, request.tariffPlan);
                plan.fk_business = businessGuid;
                plan.updated_at = DateTime.UtcNow;
                await db.SaveChangesAsync();

                var existingPeriods = await db.tbl_tariff_time_period
                    .Where(x => x.fk_tariff_plan == plan.energy_tariff_plan_id && !x.is_deleted)
                    .ToListAsync();
                foreach (var old in existingPeriods)
                {
                    old.is_deleted = true;
                    old.updated_at = DateTime.UtcNow;
                }

                foreach (var dto in request.tariffPlan.timePeriods)
                {
                    var period = new tbl_tariff_time_period
                    {
                        fk_tariff_plan = plan.energy_tariff_plan_id
                    };
                    ApplyPeriod(period, dto);
                    await db.tbl_tariff_time_period.AddAsync(period);
                }

                var demand = await db.tbl_demand_management_setting
                    .FirstOrDefaultAsync(x => x.fk_business == businessGuid && !x.is_deleted);
                if (demand == null)
                {
                    demand = new tbl_demand_management_setting { fk_business = businessGuid };
                    await db.tbl_demand_management_setting.AddAsync(demand);
                }

                ApplyDemand(demand, request.demandManagement);
                demand.fk_business = businessGuid;
                demand.updated_at = DateTime.UtcNow;

                await SyncLegacySetting(businessGuid, request.tariffPlan);
                await db.SaveChangesAsync();
                await tx.CommitAsync();

                try
                {
                    await demandRedisCache.SetBusinessAsync(businessGuid);
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Demand-management Redis cache refresh failed for business {BusinessId}.",
                        businessGuid);
                }

                return await GetByBusinessId(request.fkBusiness);
            }
            catch (Exception exception)
            {
                await tx.RollbackAsync();
                logger.LogError(exception, "Energy configuration save failed for business {BusinessId}.", businessGuid);
                return Fail("Energy configuration could not be saved.");
            }
        }

        private async Task SyncLegacySetting(Guid businessId, EnergyTariffPlanDTO dto)
        {
            var legacy = await db.tbl_business_dashboard_setting
                .FirstOrDefaultAsync(x => x.fk_business == businessId && !x.is_deleted);
            if (legacy == null)
            {
                legacy = new tbl_business_dashboard_setting { fk_business = businessId };
                await db.tbl_business_dashboard_setting.AddAsync(legacy);
            }

            var peak = dto.timePeriods.FirstOrDefault(x =>
                x.isActive && x.periodType.Equals("Peak", StringComparison.OrdinalIgnoreCase));

            legacy.currency = dto.currency;
            // The reporting timezone is no longer part of the tariff form. Do not silently
            // overwrite the legacy/site timezone while saving tariff rates.
            legacy.tariff_rate = dto.standardRatePerKwh;
            legacy.peak_tariff_rate = dto.peakRatePerKwh;
            legacy.off_peak_tariff_rate = dto.offPeakRatePerKwh;
            if (peak != null)
            {
                legacy.peak_start_time = peak.startTime;
                legacy.peak_end_time = peak.endTime;
                legacy.day_of_week = peak.dayOfWeek;
            }

            legacy.is_active = true;
            legacy.updated_at = DateTime.UtcNow;
        }

        private static string Validate(EnergyConfigurationDTO request)
        {
            if (request.tariffPlan == null || request.demandManagement == null)
                return "Tariff and demand settings are required.";
            if (string.IsNullOrWhiteSpace(request.tariffPlan.planName))
                return "Tariff plan name is required.";
            if (request.tariffPlan.standardRatePerKwh < 0 ||
                request.tariffPlan.peakRatePerKwh < 0 ||
                request.tariffPlan.offPeakRatePerKwh < 0)
                return "Tariff rates cannot be negative.";
            if (request.demandManagement.demandLimitKw <= 0)
                return "Demand limit must be greater than zero.";
            if (request.demandManagement.recoveryThresholdKw >= request.demandManagement.demandLimitKw)
                return "Recovery threshold must be lower than the demand limit.";

            foreach (var period in request.tariffPlan.timePeriods.Where(x => x.isActive))
            {
                if (!TimeOnly.TryParse(period.startTime, out _) || !TimeOnly.TryParse(period.endTime, out _))
                    return "Every active tariff period must contain valid start and end times.";
            }

            return string.Empty;
        }

        private static void ApplyPlan(tbl_energy_tariff_plan entity, EnergyTariffPlanDTO dto)
        {
            entity.plan_name = dto.planName.Trim();
            entity.currency = dto.currency.Trim().ToUpperInvariant();
            entity.standard_rate_per_kwh = dto.standardRatePerKwh;
            entity.peak_rate_per_kwh = dto.peakRatePerKwh;
            entity.off_peak_rate_per_kwh = dto.offPeakRatePerKwh;
            entity.demand_charge_per_kw = dto.demandChargePerKw;
            entity.is_active = dto.isActive;
        }

        private static void ApplyPeriod(tbl_tariff_time_period entity, TariffTimePeriodDTO dto)
        {
            entity.period_name = dto.periodName.Trim();
            entity.period_type = dto.periodType.Trim();
            entity.start_time = TimeOnly.Parse(dto.startTime);
            entity.end_time = TimeOnly.Parse(dto.endTime);
            entity.day_of_week = dto.dayOfWeek;
            entity.season_start = DateOnly.TryParse(dto.seasonStart, out var start) ? start : null;
            entity.season_end = DateOnly.TryParse(dto.seasonEnd, out var end) ? end : null;
            entity.is_active = dto.isActive;
            entity.updated_at = DateTime.UtcNow;
        }

        private static void ApplyDemand(tbl_demand_management_setting entity, DemandManagementSettingDTO dto)
        {
            entity.demand_limit_kw = dto.demandLimitKw;
            entity.warning_threshold_percent = dto.warningThresholdPercent;
            entity.recovery_threshold_kw = dto.recoveryThresholdKw;
            entity.demand_interval_minutes = dto.demandIntervalMinutes;
            entity.stabilization_minutes = dto.stabilizationMinutes;
            entity.enable_peak_hour_control = dto.enablePeakHourControl;
            entity.enable_demand_threshold_control = dto.enableDemandThresholdControl;
            entity.suggestion_only_mode = dto.suggestionOnlyMode;
            entity.is_active = dto.isActive;
        }

        private static EnergyTariffPlanDTO MapPlan(tbl_energy_tariff_plan entity) => new()
        {
            energyTariffPlanId = entity.energy_tariff_plan_id.ToString(),
            fkBusiness = entity.fk_business.ToString(),
            planName = entity.plan_name,
            currency = entity.currency,
            standardRatePerKwh = entity.standard_rate_per_kwh,
            peakRatePerKwh = entity.peak_rate_per_kwh,
            offPeakRatePerKwh = entity.off_peak_rate_per_kwh,
            demandChargePerKw = entity.demand_charge_per_kw,
            isActive = entity.is_active
        };

        private static TariffTimePeriodDTO MapPeriod(tbl_tariff_time_period entity) => new()
        {
            tariffTimePeriodId = entity.tariff_time_period_id.ToString(),
            periodName = entity.period_name,
            periodType = entity.period_type,
            startTime = entity.start_time.ToString("HH:mm"),
            endTime = entity.end_time.ToString("HH:mm"),
            dayOfWeek = entity.day_of_week,
            seasonStart = entity.season_start?.ToString("yyyy-MM-dd"),
            seasonEnd = entity.season_end?.ToString("yyyy-MM-dd"),
            isActive = entity.is_active
        };

        private static DemandManagementSettingDTO MapDemand(tbl_demand_management_setting entity) => new()
        {
            demandManagementSettingId = entity.demand_management_setting_id.ToString(),
            fkBusiness = entity.fk_business.ToString(),
            demandLimitKw = entity.demand_limit_kw,
            warningThresholdPercent = entity.warning_threshold_percent,
            recoveryThresholdKw = entity.recovery_threshold_kw,
            demandIntervalMinutes = entity.demand_interval_minutes,
            stabilizationMinutes = entity.stabilization_minutes,
            enablePeakHourControl = entity.enable_peak_hour_control,
            enableDemandThresholdControl = entity.enable_demand_threshold_control,
            suggestionOnlyMode = entity.suggestion_only_mode,
            isActive = entity.is_active
        };

        private static ResponseModel<EnergyConfigurationDTO> Fail(string message) => new()
        {
            remarks = message,
            success = false
        };
    }
}
