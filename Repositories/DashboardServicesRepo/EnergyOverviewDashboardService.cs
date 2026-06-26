////using EMO.Models.DBModels;
////using EMO.Models.DTOs.DashboardDTOs;
////using EnergyMonitor.DTOs;
////using Microsoft.EntityFrameworkCore;
////using System;

////namespace EMO.Repositories.DashboardServicesRepo
////{
////    public class EnergyOverviewDashboardService : IEnergyOverviewDashboardService
////    {
////        private readonly DBUserManagementContext _db;

////        public EnergyOverviewDashboardService(DBUserManagementContext db)
////        {
////            _db = db;
////        }

////        private static (DateTime from, DateTime to) ResolveRange(DashboardQueryParams q)
////        {
////            var to = q.To ?? DateTime.UtcNow;

////            var from = q.From ?? q.Range switch
////            {
////                "7d" => to.AddDays(-7),
////                "30d" => to.AddDays(-30),
////                _ => to.AddHours(-24)
////            };

////            return (from, to);
////        }

////        public async Task<EnergyOverviewDashboardDto> GetBusinessOverviewAsync(
////            Guid businessId,
////            DashboardQueryParams q)
////        {
////            var (from, to) = ResolveRange(q);

////            var sensorIds = _db.tbl_sensor
////                .Where(s => !s.is_deleted && s.device.fk_business == businessId)
////                .Select(s => s.sensor_id);

////            var data = await _db.tbl_singal_phase_data
////                .Where(x =>
////                    !x.is_deleted &&
////                    sensorIds.Contains(x.fk_sensor) &&
////                    x.created_at >= from &&
////                    x.created_at <= to)
////                .Select(x => new
////                {
////                    x.created_at,
////                    x.volt,
////                    x.active_power,
////                    x.active_energy,
////                    x.power_factor,
////                    SensorName = x.sensor.sensor_name,
////                    UtilityName = x.sensor.utility.utility_name
////                })
////                .ToListAsync();

////            var totalEnergyKwh = Math.Round(data.Sum(x => (double)x.active_energy) / 1000, 2);

////            var monthlyTargetKwh = 2000;
////            var usagePercent = monthlyTargetKwh > 0
////                ? Math.Round((totalEnergyKwh / monthlyTargetKwh) * 100, 1)
////                : 0;

////            var groupedByType = data
////                .GroupBy(x => NormalizeType(x.UtilityName))
////                .ToList();

////            var summaries = groupedByType
////                .Select(g => new DeviceTypeSummaryDto
////                {
////                    DeviceType = g.Key,
////                    TotalEnergyKwh = Math.Round(g.Sum(x => (double)x.active_energy) / 1000, 2),
////                    AvgVoltage = Math.Round(g.Average(x => (double)x.volt), 2),
////                    AvgActivePowerW = Math.Round(g.Average(x => (double)x.active_power), 2),
////                    ChangePercent = 0,
////                    Sparkline = g
////                        .GroupBy(x => x.created_at.Hour)
////                        .OrderBy(x => x.Key)
////                        .Select(x => Math.Round(x.Average(v => (double)v.active_power), 2))
////                        .Take(12)
////                        .ToList()
////                })
////                .ToList();

////            var latestMeters = data
////                .GroupBy(x => x.SensorName)
////                .Select(g =>
////                {
////                    var latest = g.OrderByDescending(x => x.created_at).First();

////                    return new EnergyMeterOverviewDto
////                    {
////                        MeterName = g.Key,
////                        Voltage = Math.Round((double)latest.volt, 2),
////                        PowerW = Math.Round((double)latest.active_power, 2)
////                    };
////                })
////                .Take(5)
////                .ToList();

////            var monthly = data
////                .GroupBy(x => new { x.created_at.Year, x.created_at.Month })
////                .OrderBy(x => x.Key.Year)
////                .ThenBy(x => x.Key.Month)
////                .Take(12)
////                .Select(g => new MonthlyEnergyDeviceTypeDto
////                {
////                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
////                    Hvac = Math.Round(g.Where(x => NormalizeType(x.UtilityName) == "HVAC").Sum(x => (double)x.active_energy) / 1000, 2),
////                    Lighting = Math.Round(g.Where(x => NormalizeType(x.UtilityName) == "Lighting").Sum(x => (double)x.active_energy) / 1000, 2),
////                    Miscellaneous = Math.Round(g.Where(x => NormalizeType(x.UtilityName) == "Miscellaneous").Sum(x => (double)x.active_energy) / 1000, 2),
////                    Computation = Math.Round(g.Where(x => NormalizeType(x.UtilityName) == "Computation").Sum(x => (double)x.active_energy) / 1000, 2)
////                })
////                .ToList();

////            var distribution = BuildDistribution(summaries);

////            var critical = data.Count(x => x.volt < 210 || x.volt > 250);
////            var warning = data.Count(x => x.power_factor < 0.85);
////            var info = data.Count(x => x.power_factor >= 0.85 && x.power_factor < 0.90);

////            return new EnergyOverviewDashboardDto
////            {
////                MonthlyTargetKwh = monthlyTargetKwh,
////                CurrentUsageKwh = totalEnergyKwh,
////                UsagePercent = usagePercent,
////                DeviceTypeSummaries = summaries,
////                EnergyMeters = latestMeters,
////                MonthlyDeviceTypeConsumption = monthly,
////                DeviceTypeDistribution = distribution,
////                Alerts = new AlertOverviewDto
////                {
////                    CriticalAlerts = critical,
////                    WarningAlerts = warning,
////                    InfoAlerts = info,
////                    TotalAlerts = critical + warning + info
////                }
////            };
////        }

////        private static string NormalizeType(string? type)
////        {
////            if (string.IsNullOrWhiteSpace(type))
////                return "Miscellaneous";

////            type = type.ToLower();

////            if (type.Contains("hvac"))
////                return "HVAC";

////            if (type.Contains("light"))
////                return "Lighting";

////            if (type.Contains("comput"))
////                return "Computation";

////            return "Miscellaneous";
////        }

////        private static DeviceTypeDistributionDto BuildDistribution(List<DeviceTypeSummaryDto> summaries)
////        {
////            var total = summaries.Sum(x => x.TotalEnergyKwh);

////            if (total <= 0)
////                return new DeviceTypeDistributionDto();

////            double Pct(string type)
////            {
////                return Math.Round(
////                    summaries.Where(x => x.DeviceType == type).Sum(x => x.TotalEnergyKwh) * 100 / total,
////                    1
////                );
////            }

////            return new DeviceTypeDistributionDto
////            {
////                HvacPct = Pct("HVAC"),
////                LightingPct = Pct("Lighting"),
////                MiscellaneousPct = Pct("Miscellaneous"),
////                ComputationPct = Pct("Computation")
////            };
////        }
////    }
////}


//using EMO.Models.DBModels;
//using EMO.Models.DTOs.DashboardDTOs;
//using EnergyMonitor.DTOs;
//using Microsoft.EntityFrameworkCore;
//using System;

//namespace EMO.Repositories.DashboardServicesRepo
//{
//    public class EnergyOverviewDashboardService : IEnergyOverviewDashboardService
//    {
//        private readonly DBUserManagementContext _db;

//        public EnergyOverviewDashboardService(DBUserManagementContext db)
//        {
//            _db = db;
//        }

//        private static (DateTime from, DateTime to) ResolveRange(DashboardQueryParams q)
//        {
//            var to = q.To ?? DateTime.UtcNow;

//            var from = q.From ?? q.Range switch
//            {
//                "7d" => to.AddDays(-7),
//                "30d" => to.AddDays(-30),
//                _ => to.AddHours(-24)
//            };

//            return (from, to);
//        }

//        /*
//         * IMPORTANT:
//         * active_energy is a cumulative meter reading.
//         * So we should NOT do Sum(active_energy).
//         *
//         * Correct method:
//         * 1. Group readings sensor-wise.
//         * 2. Order each sensor readings by created_at.
//         * 3. Calculate current.active_energy - previous.active_energy.
//         * 4. Ignore negative/zero values because meter reset can happen.
//         * 5. Sum positive differences.
//         */
//        private static double CalculateSensorWiseEnergyKwh<T>(IEnumerable<T> rows)
//        {
//            double totalKwh = 0;

//            var sensorGroups = rows
//                .Cast<dynamic>()
//                .GroupBy(x => x.fk_sensor);

//            foreach (var sensorGroup in sensorGroups)
//            {
//                var orderedRows = sensorGroup
//                    .OrderBy(x => x.created_at)
//                    .ToList();

//                if (orderedRows.Count < 2)
//                    continue;

//                for (int i = 1; i < orderedRows.Count; i++)
//                {
//                    double previousEnergy = Convert.ToDouble(orderedRows[i - 1].active_energy);
//                    double currentEnergy = Convert.ToDouble(orderedRows[i].active_energy);

//                    double consumption = currentEnergy - previousEnergy;

//                    if (consumption > 0)
//                    {
//                        totalKwh += consumption;
//                    }
//                }
//            }

//            return totalKwh;
//        }

//        private static List<double> BuildEnergySparkline<T>(IEnumerable<T> rows)
//        {
//            var hourlyTotals = new Dictionary<int, double>();

//            var sensorGroups = rows
//                .Cast<dynamic>()
//                .GroupBy(x => x.fk_sensor);

//            foreach (var sensorGroup in sensorGroups)
//            {
//                var orderedRows = sensorGroup
//                    .OrderBy(x => x.created_at)
//                    .ToList();

//                if (orderedRows.Count < 2)
//                    continue;

//                for (int i = 1; i < orderedRows.Count; i++)
//                {
//                    var previous = orderedRows[i - 1];
//                    var current = orderedRows[i];

//                    double previousEnergy = Convert.ToDouble(previous.active_energy);
//                    double currentEnergy = Convert.ToDouble(current.active_energy);

//                    double consumption = currentEnergy - previousEnergy;

//                    if (consumption <= 0)
//                        continue;

//                    int hour = current.created_at.Hour;

//                    if (!hourlyTotals.ContainsKey(hour))
//                    {
//                        hourlyTotals[hour] = 0;
//                    }

//                    hourlyTotals[hour] += consumption;
//                }
//            }

//            return hourlyTotals
//                .OrderBy(x => x.Key)
//                .Select(x => Math.Round(x.Value, 2))
//                .Take(12)
//                .ToList();
//        }

//        public async Task<EnergyOverviewDashboardDto> GetBusinessOverviewAsync(
//            Guid businessId,
//            DashboardQueryParams q)
//        {
//            var (from, to) = ResolveRange(q);

//            var sensorIds = _db.tbl_sensor
//                .Where(s => !s.is_deleted && s.device.fk_business == businessId)
//                .Select(s => s.sensor_id);

//            var data = await _db.tbl_singal_phase_data
//                .Where(x =>
//                    !x.is_deleted &&
//                    sensorIds.Contains(x.fk_sensor) &&
//                    x.created_at >= from &&
//                    x.created_at <= to)
//                .Select(x => new
//                {
//                    x.fk_sensor,
//                    x.created_at,
//                    x.volt,
//                    x.active_power,
//                    x.active_energy,
//                    x.power_factor,
//                    SensorName = x.sensor.sensor_name,
//                    UtilityName = x.sensor.utility.utility_name
//                })
//                .ToListAsync();

//            var totalEnergyKwh = Math.Round(CalculateSensorWiseEnergyKwh(data), 2);

//            var monthlyTargetKwh = 2000;
//            var usagePercent = monthlyTargetKwh > 0
//                ? Math.Round((totalEnergyKwh / monthlyTargetKwh) * 100, 1)
//                : 0;

//            var groupedByType = data
//                .GroupBy(x => NormalizeType(x.UtilityName))
//                .ToList();

//            var summaries = groupedByType
//                .Select(g =>
//                {
//                    var groupRows = g.ToList();

//                    return new DeviceTypeSummaryDto
//                    {
//                        DeviceType = g.Key,
//                        TotalEnergyKwh = Math.Round(CalculateSensorWiseEnergyKwh(groupRows), 2),
//                        AvgVoltage = Math.Round(groupRows.Average(x => (double)x.volt), 2),
//                        AvgActivePowerW = Math.Round(groupRows.Average(x => (double)x.active_power), 2),
//                        ChangePercent = 0,

//                        // This sparkline now shows hourly energy consumption, not average power.
//                        Sparkline = BuildEnergySparkline(groupRows)
//                    };
//                })
//                .ToList();

//            var latestMeters = data
//                .GroupBy(x => x.SensorName)
//                .Select(g =>
//                {
//                    var latest = g.OrderByDescending(x => x.created_at).First();

//                    return new EnergyMeterOverviewDto
//                    {
//                        MeterName = g.Key,
//                        Voltage = Math.Round((double)latest.volt, 2),
//                        PowerW = Math.Round((double)latest.active_power, 2)
//                    };
//                })
//                .Take(5)
//                .ToList();

//            var monthly = data
//                .GroupBy(x => new { x.created_at.Year, x.created_at.Month })
//                .OrderBy(x => x.Key.Year)
//                .ThenBy(x => x.Key.Month)
//                .Take(12)
//                .Select(g =>
//                {
//                    var monthRows = g.ToList();

//                    return new MonthlyEnergyDeviceTypeDto
//                    {
//                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),

//                        Hvac = Math.Round(
//                            CalculateSensorWiseEnergyKwh(
//                                monthRows.Where(x => NormalizeType(x.UtilityName) == "HVAC")
//                            ),
//                            2
//                        ),

//                        Lighting = Math.Round(
//                            CalculateSensorWiseEnergyKwh(
//                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Lighting")
//                            ),
//                            2
//                        ),

//                        Miscellaneous = Math.Round(
//                            CalculateSensorWiseEnergyKwh(
//                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Miscellaneous")
//                            ),
//                            2
//                        ),

//                        Computation = Math.Round(
//                            CalculateSensorWiseEnergyKwh(
//                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Computation")
//                            ),
//                            2
//                        )
//                    };
//                })
//                .ToList();

//            var distribution = BuildDistribution(summaries);

//            var critical = data.Count(x => x.volt < 210 || x.volt > 250);
//            var warning = data.Count(x => x.power_factor < 0.85);
//            var info = data.Count(x => x.power_factor >= 0.85 && x.power_factor < 0.90);

//            return new EnergyOverviewDashboardDto
//            {
//                MonthlyTargetKwh = monthlyTargetKwh,
//                CurrentUsageKwh = totalEnergyKwh,
//                UsagePercent = usagePercent,
//                DeviceTypeSummaries = summaries,
//                EnergyMeters = latestMeters,
//                MonthlyDeviceTypeConsumption = monthly,
//                DeviceTypeDistribution = distribution,
//                Alerts = new AlertOverviewDto
//                {
//                    CriticalAlerts = critical,
//                    WarningAlerts = warning,
//                    InfoAlerts = info,
//                    TotalAlerts = critical + warning + info
//                }
//            };
//        }

//        private static string NormalizeType(string? type)
//        {
//            if (string.IsNullOrWhiteSpace(type))
//                return "Miscellaneous";

//            type = type.ToLower();

//            if (type.Contains("hvac"))
//                return "HVAC";

//            if (type.Contains("light"))
//                return "Lighting";

//            if (type.Contains("comput"))
//                return "Computation";

//            return "Miscellaneous";
//        }

//        private static DeviceTypeDistributionDto BuildDistribution(List<DeviceTypeSummaryDto> summaries)
//        {
//            var total = summaries.Sum(x => x.TotalEnergyKwh);

//            if (total <= 0)
//                return new DeviceTypeDistributionDto();

//            double Pct(string type)
//            {
//                return Math.Round(
//                    summaries.Where(x => x.DeviceType == type).Sum(x => x.TotalEnergyKwh) * 100 / total,
//                    1
//                );
//            }

//            return new DeviceTypeDistributionDto
//            {
//                HvacPct = Pct("HVAC"),
//                LightingPct = Pct("Lighting"),
//                MiscellaneousPct = Pct("Miscellaneous"),
//                ComputationPct = Pct("Computation")
//            };
//        }
//    }
//}


using EMO.Models.DBModels;
using EMO.Models.DTOs.DashboardDTOs;
using EnergyMonitor.DTOs;
using Microsoft.EntityFrameworkCore;
using System;

namespace EMO.Repositories.DashboardServicesRepo
{
    public class EnergyOverviewDashboardService : IEnergyOverviewDashboardService
    {
        private readonly DBUserManagementContext _db;

        public EnergyOverviewDashboardService(DBUserManagementContext db)
        {
            _db = db;
        }

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

        /*
         * active_energy is cumulative.
         * Correct consumption = current.active_energy - previous.active_energy.
         * We calculate this sensor-wise to avoid mixing readings from different meters.
         */
        private static double CalculateSensorWiseEnergyKwh<T>(IEnumerable<T> rows)
        {
            double totalKwh = 0;

            var sensorGroups = rows
                .Cast<dynamic>()
                .GroupBy(x => x.fk_sensor);

            foreach (var sensorGroup in sensorGroups)
            {
                var orderedRows = sensorGroup
                    .OrderBy(x => x.created_at)
                    .ToList();

                if (orderedRows.Count < 2)
                    continue;

                for (int i = 1; i < orderedRows.Count; i++)
                {
                    double previousEnergy = Convert.ToDouble(orderedRows[i - 1].active_energy);
                    double currentEnergy = Convert.ToDouble(orderedRows[i].active_energy);

                    double consumption = currentEnergy - previousEnergy;

                    if (consumption > 0)
                    {
                        totalKwh += consumption;
                    }
                }
            }

            return totalKwh;
        }

        private static List<double> BuildEnergySparkline<T>(IEnumerable<T> rows)
        {
            var hourlyTotals = new Dictionary<int, double>();

            var sensorGroups = rows
                .Cast<dynamic>()
                .GroupBy(x => x.fk_sensor);

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

                    double previousEnergy = Convert.ToDouble(previous.active_energy);
                    double currentEnergy = Convert.ToDouble(current.active_energy);

                    double consumption = currentEnergy - previousEnergy;

                    if (consumption <= 0)
                        continue;

                    int hour = current.created_at.Hour;

                    if (!hourlyTotals.ContainsKey(hour))
                    {
                        hourlyTotals[hour] = 0;
                    }

                    hourlyTotals[hour] += consumption;
                }
            }

            return hourlyTotals
                .OrderBy(x => x.Key)
                .Select(x => Math.Round(x.Value, 2))
                .Take(12)
                .ToList();
        }

        private IQueryable<Guid> SensorsByBusiness(Guid businessId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted && s.device.fk_business == businessId)
                .Select(s => s.sensor_id);
        }

        private IQueryable<Guid> SensorsByFacility(Guid facilityId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.section.floor.building.fk_facility == facilityId)
                .Select(s => s.sensor_id);
        }

        private IQueryable<Guid> SensorsByBuilding(Guid buildingId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.section.floor.fk_building == buildingId)
                .Select(s => s.sensor_id);
        }

        private IQueryable<Guid> SensorsByFloor(Guid floorId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.section.fk_floor == floorId)
                .Select(s => s.sensor_id);
        }

        private IQueryable<Guid> SensorsBySection(Guid sectionId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.office.fk_section == sectionId)
                .Select(s => s.sensor_id);
        }

        private IQueryable<Guid> SensorsByOffice(Guid officeId)
        {
            return _db.tbl_sensor
                .Where(s => !s.is_deleted &&
                            s.device.fk_office == officeId)
                .Select(s => s.sensor_id);
        }

        /*
         * New common method for all hierarchy levels.
         * Frontend route:
         * /dashboard/overview/{level}/{id}
         */
        public async Task<EnergyOverviewDashboardDto> GetOverviewAsync(
            string level,
            Guid id,
            DashboardQueryParams q)
        {
            var sensorIds = level.ToLower() switch
            {
                "business" => SensorsByBusiness(id),
                "facility" => SensorsByFacility(id),
                "building" => SensorsByBuilding(id),
                "floor" => SensorsByFloor(id),
                "section" => SensorsBySection(id),
                "office" => SensorsByOffice(id),
                _ => throw new ArgumentException("Invalid overview level.")
            };

            return await BuildOverviewAsync(sensorIds, q);
        }

        /*
         * Keep old business method also, so existing code will not break.
         */
        public async Task<EnergyOverviewDashboardDto> GetBusinessOverviewAsync(
            Guid businessId,
            DashboardQueryParams q)
        {
            return await BuildOverviewAsync(SensorsByBusiness(businessId), q);
        }

        private async Task<EnergyOverviewDashboardDto> BuildOverviewAsync(
            IQueryable<Guid> sensorIds,
            DashboardQueryParams q)
        {
            var (from, to) = ResolveRange(q);

            var data = await _db.tbl_singal_phase_data
                .Where(x =>
                    !x.is_deleted &&
                    sensorIds.Contains(x.fk_sensor) &&
                    x.created_at >= from &&
                    x.created_at <= to)
                .Select(x => new
                {
                    x.fk_sensor,
                    x.created_at,
                    x.volt,
                    x.active_power,
                    x.active_energy,
                    x.power_factor,
                    SensorName = x.sensor.sensor_name,
                    UtilityName = x.sensor.utility.utility_name
                })
                .ToListAsync();

            var totalEnergyKwh = Math.Round(CalculateSensorWiseEnergyKwh(data), 2);

            var monthlyTargetKwh = 2000;
            var usagePercent = monthlyTargetKwh > 0
                ? Math.Round((totalEnergyKwh / monthlyTargetKwh) * 100, 1)
                : 0;

            var groupedByType = data
                .GroupBy(x => NormalizeType(x.UtilityName))
                .ToList();

            var summaries = groupedByType
                .Select(g =>
                {
                    var groupRows = g.ToList();

                    return new DeviceTypeSummaryDto
                    {
                        DeviceType = g.Key,
                        TotalEnergyKwh = Math.Round(CalculateSensorWiseEnergyKwh(groupRows), 2),
                        AvgVoltage = groupRows.Any()
                            ? Math.Round(groupRows.Average(x => (double)x.volt), 2)
                            : 0,
                        AvgActivePowerW = groupRows.Any()
                            ? Math.Round(groupRows.Average(x => (double)x.active_power), 2)
                            : 0,
                        ChangePercent = 0,
                        Sparkline = BuildEnergySparkline(groupRows)
                    };
                })
                .ToList();

            var latestMeters = data
                .GroupBy(x => x.SensorName)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.created_at).First();

                    return new EnergyMeterOverviewDto
                    {
                        MeterName = g.Key,
                        Voltage = Math.Round((double)latest.volt, 2),
                        PowerW = Math.Round((double)latest.active_power, 2)
                    };
                })
                .Take(5)
                .ToList();

            var monthly = data
                .GroupBy(x => new { x.created_at.Year, x.created_at.Month })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Take(12)
                .Select(g =>
                {
                    var monthRows = g.ToList();

                    return new MonthlyEnergyDeviceTypeDto
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),

                        Hvac = Math.Round(
                            CalculateSensorWiseEnergyKwh(
                                monthRows.Where(x => NormalizeType(x.UtilityName) == "HVAC")
                            ),
                            2
                        ),

                        Lighting = Math.Round(
                            CalculateSensorWiseEnergyKwh(
                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Lighting")
                            ),
                            2
                        ),

                        Miscellaneous = Math.Round(
                            CalculateSensorWiseEnergyKwh(
                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Miscellaneous")
                            ),
                            2
                        ),

                        Computation = Math.Round(
                            CalculateSensorWiseEnergyKwh(
                                monthRows.Where(x => NormalizeType(x.UtilityName) == "Computation")
                            ),
                            2
                        )
                    };
                })
                .ToList();

            var distribution = BuildDistribution(summaries);

            var critical = data.Count(x => x.volt < 210 || x.volt > 250);
            var warning = data.Count(x => x.power_factor < 0.85);
            var info = data.Count(x => x.power_factor >= 0.85 && x.power_factor < 0.90);

            return new EnergyOverviewDashboardDto
            {
                MonthlyTargetKwh = monthlyTargetKwh,
                CurrentUsageKwh = totalEnergyKwh,
                UsagePercent = usagePercent,
                DeviceTypeSummaries = summaries,
                EnergyMeters = latestMeters,
                MonthlyDeviceTypeConsumption = monthly,
                DeviceTypeDistribution = distribution,
                Alerts = new AlertOverviewDto
                {
                    CriticalAlerts = critical,
                    WarningAlerts = warning,
                    InfoAlerts = info,
                    TotalAlerts = critical + warning + info
                }
            };
        }

        private static string NormalizeType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Miscellaneous";

            type = type.ToLower();

            if (type.Contains("hvac"))
                return "HVAC";

            if (type.Contains("light"))
                return "Lighting";

            if (type.Contains("comput"))
                return "Computation";

            return "Miscellaneous";
        }

        private static DeviceTypeDistributionDto BuildDistribution(List<DeviceTypeSummaryDto> summaries)
        {
            var total = summaries.Sum(x => x.TotalEnergyKwh);

            if (total <= 0)
                return new DeviceTypeDistributionDto();

            double Pct(string type)
            {
                return Math.Round(
                    summaries.Where(x => x.DeviceType == type).Sum(x => x.TotalEnergyKwh) * 100 / total,
                    1
                );
            }

            return new DeviceTypeDistributionDto
            {
                HvacPct = Pct("HVAC"),
                LightingPct = Pct("Lighting"),
                MiscellaneousPct = Pct("Miscellaneous"),
                ComputationPct = Pct("Computation")
            };
        }
    }
}

