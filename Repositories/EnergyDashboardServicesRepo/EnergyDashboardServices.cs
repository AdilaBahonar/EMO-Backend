using System.Text;
using EMO.Models.DBModels;
using EMO.Models.DTOs.EnergyDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.EnergyDashboardServicesRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.EnergyDashboardRepo
{
    public class EnergyDashboardService : IEnergyDashboardService
    {
        private readonly DBUserManagementContext db;

        public EnergyDashboardService(DBUserManagementContext db)
        {
            this.db = db;
        }

        public async Task<ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>> GetMonthlyDeviceTypeReport()
        {
            try
            {
                var now = DateTime.UtcNow;

                var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);

                var rows = await db.tbl_singal_phase_data
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.utility)
                    .Where(x => !x.is_deleted &&
                                x.created_at >= startDate &&
                                x.created_at < endDate &&
                                x.sensor != null &&
                                x.sensor.utility != null &&
                                x.fk_sensor != Guid.Empty)
                    .Select(x => new
                    {
                        x.fk_sensor,
                        x.created_at,
                        x.active_energy,
                        utilityName = x.sensor.utility.utility_name
                    })
                    .ToListAsync();

                var sensorWiseData = rows
                    .GroupBy(x => new
                    {
                        x.fk_sensor,
                        x.utilityName
                    })
                    .Select(g =>
                    {
                        var orderedRows = g.OrderBy(x => x.created_at).ToList();

                        float sensorTotalKwh = 0;

                        for (int i = 1; i < orderedRows.Count; i++)
                        {
                            var previous = orderedRows[i - 1];
                            var current = orderedRows[i];

                            var consumption = current.active_energy - previous.active_energy;

                            if (consumption > 0)
                            {
                                sensorTotalKwh += consumption;
                            }
                        }

                        return new
                        {
                            g.Key.utilityName,
                            totalKwh = sensorTotalKwh
                        };
                    })
                    .ToList();

                var rawData = sensorWiseData
                    .GroupBy(x => x.utilityName)
                    .Select(g => new
                    {
                        utilityName = g.Key,
                        totalKwh = g.Sum(x => x.totalKwh)
                    })
                    .ToList();

                var total = rawData.Sum(x => x.totalKwh);

                var result = rawData.Select(x => new MonthlyDeviceTypeReportResponseDTO
                {
                    utilityName = x.utilityName,
                    totalKwh = x.totalKwh,
                    percentage = total > 0 ? (x.totalKwh / total) * 100 : 0
                }).ToList();

                return new ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>
                {
                    data = result,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<MonthlyDeviceTypeReportResponseDTO>>
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }


        public async Task<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>> GetEnergyConsumptionByDeviceTypeLast12Months()
        {
            try
            {
                var now = DateTime.UtcNow;

                var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMonths(-11);

                var rows = await db.tbl_singal_phase_data
                    .Include(x => x.sensor)
                    .ThenInclude(x => x.utility)
                    .Where(x => !x.is_deleted &&
                                x.created_at >= startDate &&
                                x.sensor != null &&
                                x.sensor.utility != null &&
                                x.fk_sensor != Guid.Empty)
                    .Select(x => new
                    {
                        x.fk_sensor,
                        x.created_at,
                        x.active_energy,
                        year = x.created_at.Year,
                        monthNumber = x.created_at.Month,
                        utilityName = x.sensor.utility.utility_name
                    })
                    .ToListAsync();

                var sensorWiseMonthlyData = rows
                    .GroupBy(x => new
                    {
                        x.fk_sensor,
                        x.year,
                        x.monthNumber,
                        x.utilityName
                    })
                    .Select(g =>
                    {
                        var orderedRows = g.OrderBy(x => x.created_at).ToList();

                        float sensorTotalKwh = 0;

                        for (int i = 1; i < orderedRows.Count; i++)
                        {
                            var previous = orderedRows[i - 1];
                            var current = orderedRows[i];

                            var consumption = current.active_energy - previous.active_energy;

                            if (consumption > 0)
                            {
                                sensorTotalKwh += consumption;
                            }
                        }

                        return new
                        {
                            g.Key.year,
                            g.Key.monthNumber,
                            g.Key.utilityName,
                            totalKwh = sensorTotalKwh
                        };
                    })
                    .ToList();

                var result = sensorWiseMonthlyData
                    .GroupBy(x => new
                    {
                        x.year,
                        x.monthNumber,
                        x.utilityName
                    })
                    .Select(g => new EnergyConsumptionByDeviceTypeResponseDTO
                    {
                        year = g.Key.year,
                        month = new DateTime(g.Key.year, g.Key.monthNumber, 1, 0, 0, 0, DateTimeKind.Utc)
                            .ToString("MMM"),
                        utilityName = g.Key.utilityName,
                        totalKwh = g.Sum(x => x.totalKwh)
                    })
                    .OrderBy(x => x.year)
                    .ThenBy(x => DateTime.ParseExact(x.month, "MMM", null).Month)
                    .ToList();

                return new ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>
                {
                    data = result,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }
        //public async Task<ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>> GetEnergyConsumptionByDeviceTypeLast12Months()
        //{
        //    try
        //    {
        //        var now = DateTime.UtcNow;

        //        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
        //            .AddMonths(-11);

        //        var rows = await db.tbl_singal_phase_data
        //            .Include(x => x.sensor)
        //            .ThenInclude(x => x.utility)
        //            .Where(x => !x.is_deleted &&
        //                        x.created_at >= startDate &&
        //                        x.sensor != null &&
        //                        x.sensor.utility != null)
        //            .Select(x => new
        //            {
        //                x.created_at,
        //                x.active_energy,
        //                year = x.created_at.Year,
        //                monthNumber = x.created_at.Month,
        //                utilityName = x.sensor.utility.utility_name
        //            })
        //            .ToListAsync();

        //        var result = rows
        //            .GroupBy(x => new
        //            {
        //                x.year,
        //                x.monthNumber,
        //                x.utilityName
        //            })
        //            .Select(g =>
        //            {
        //                var ordered = g.OrderBy(x => x.created_at).ToList();

        //                var first = ordered.FirstOrDefault();
        //                var last = ordered.LastOrDefault();

        //                var totalKwh = first != null && last != null
        //                    ? last.active_energy - first.active_energy
        //                    : 0;

        //                if (totalKwh < 0) totalKwh = 0;

        //                return new EnergyConsumptionByDeviceTypeResponseDTO
        //                {
        //                    year = g.Key.year,
        //                    month = new DateTime(g.Key.year, g.Key.monthNumber, 1, 0, 0, 0, DateTimeKind.Utc)
        //                        .ToString("MMM"),
        //                    utilityName = g.Key.utilityName,
        //                    totalKwh = totalKwh
        //                };
        //            })
        //            .OrderBy(x => x.year)
        //            .ThenBy(x => DateTime.ParseExact(x.month, "MMM", null).Month)
        //            .ToList();

        //        return new ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>
        //        {
        //            data = result,
        //            remarks = "Success",
        //            success = true
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<List<EnergyConsumptionByDeviceTypeResponseDTO>>
        //        {
        //            remarks = $"Error: {ex.Message}",
        //            success = false
        //        };
        //    }
        //}

        //public async Task<ResponseModel<PeakNonPeakSummaryResponseDTO>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate)
        //{
        //    try
        //    {
        //        startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);

        //        endDate = DateTime.SpecifyKind(
        //            endDate.Date.AddDays(1).AddTicks(-1),
        //            DateTimeKind.Utc
        //        );

        //        var rows = await db.tbl_singal_phase_data
        //            .Where(x => !x.is_deleted &&
        //                        x.created_at >= startDate &&
        //                        x.created_at <= endDate)
        //            .Select(x => new
        //            {
        //                x.created_at,
        //                x.active_energy
        //            })
        //            .ToListAsync();

        //        var dailyData = rows
        //            .GroupBy(x => x.created_at.Date)
        //            .Select(g =>
        //            {
        //                var peakRows = g
        //                    .Where(x => x.created_at.Hour >= 18 && x.created_at.Hour < 23)
        //                    .OrderBy(x => x.created_at)
        //                    .ToList();

        //                var nonPeakRows = g
        //                    .Where(x => x.created_at.Hour < 18 || x.created_at.Hour >= 23)
        //                    .OrderBy(x => x.created_at)
        //                    .ToList();

        //                var peakKwh = CalculateConsumption(peakRows);
        //                var nonPeakKwh = CalculateConsumption(nonPeakRows);

        //                var totalKwh = peakKwh + nonPeakKwh;

        //                return new PeakNonPeakAnalysisResponseDTO
        //                {
        //                    period = g.Key.ToString("yyyy-MM-dd"),
        //                    peakKwh = peakKwh,
        //                    nonPeakKwh = nonPeakKwh,
        //                    totalKwh = totalKwh,
        //                    peakPercentage = totalKwh > 0 ? (peakKwh / totalKwh) * 100 : 0,
        //                    nonPeakPercentage = totalKwh > 0 ? (nonPeakKwh / totalKwh) * 100 : 0
        //                };
        //            })
        //            .OrderBy(x => x.period)
        //            .ToList();

        //        var totalPeak = dailyData.Sum(x => x.peakKwh);
        //        var totalNonPeak = dailyData.Sum(x => x.nonPeakKwh);
        //        var total = totalPeak + totalNonPeak;

        //        var response = new PeakNonPeakSummaryResponseDTO
        //        {
        //            totalPeakKwh = totalPeak,
        //            totalNonPeakKwh = totalNonPeak,
        //            totalKwh = total,
        //            peakPercentage = total > 0 ? (totalPeak / total) * 100 : 0,
        //            nonPeakPercentage = total > 0 ? (totalNonPeak / total) * 100 : 0,
        //            dailyData = dailyData
        //        };

        //        return new ResponseModel<PeakNonPeakSummaryResponseDTO>
        //        {
        //            data = response,
        //            remarks = "Success",
        //            success = true
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<PeakNonPeakSummaryResponseDTO>
        //        {
        //            remarks = $"Error: {ex.Message}",
        //            success = false
        //        };
        //    }
        //}



        public async Task<ResponseModel<PeakNonPeakSummaryResponseDTO>> GetPeakNonPeakAnalysis(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Convert selected dates to UTC range
                startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);

                endDate = DateTime.SpecifyKind(
                    endDate.Date.AddDays(1).AddTicks(-1),
                    DateTimeKind.Utc
                );

                var rows = await db.tbl_singal_phase_data
                    .Where(x => !x.is_deleted &&
                                x.created_at >= startDate &&
                                x.created_at <= endDate &&
                                x.fk_sensor != Guid.Empty)
                    .Select(x => new
                    {
                        x.fk_sensor,
                        x.created_at,
                        x.active_energy
                    })
                    .ToListAsync();

                /*
                 Dictionary structure:
                 Key   = Date
                 Value = Peak and Non-Peak kWh totals for that date
                */
                var dailyTotals = new Dictionary<DateTime, (float peakKwh, float nonPeakKwh)>();

                // Group by device/sensor first
                var sensorGroups = rows
                    .GroupBy(x => x.fk_sensor);

                foreach (var sensorGroup in sensorGroups)
                {
                    var orderedRows = sensorGroup
                        .OrderBy(x => x.created_at)
                        .ToList();

                    // Need at least 2 readings to calculate difference
                    if (orderedRows.Count < 2)
                        continue;

                    for (int i = 1; i < orderedRows.Count; i++)
                    {
                        var previous = orderedRows[i - 1];
                        var current = orderedRows[i];

                        // Energy difference for the same device/sensor
                        var consumption = current.active_energy - previous.active_energy;

                        // Ignore negative or zero values
                        // Negative can happen if meter resets
                        if (consumption <= 0)
                            continue;

                        // Convert UTC time to Pakistan local time
                        var localTime = current.created_at.AddHours(5);

                        var day = localTime.Date;

                        bool isPeakHour = localTime.Hour >= 18 && localTime.Hour < 23;

                        if (!dailyTotals.ContainsKey(day))
                        {
                            dailyTotals[day] = (0, 0);
                        }

                        var existing = dailyTotals[day];

                        if (isPeakHour)
                        {
                            existing.peakKwh += consumption;
                        }
                        else
                        {
                            existing.nonPeakKwh += consumption;
                        }

                        dailyTotals[day] = existing;
                    }
                }

                var dailyData = dailyTotals
                    .Select(x =>
                    {
                        var totalKwh = x.Value.peakKwh + x.Value.nonPeakKwh;

                        return new PeakNonPeakAnalysisResponseDTO
                        {
                            period = x.Key.ToString("yyyy-MM-dd"),
                            peakKwh = x.Value.peakKwh,
                            nonPeakKwh = x.Value.nonPeakKwh,
                            totalKwh = totalKwh,
                            peakPercentage = totalKwh > 0
                                ? (x.Value.peakKwh / totalKwh) * 100
                                : 0,
                            nonPeakPercentage = totalKwh > 0
                                ? (x.Value.nonPeakKwh / totalKwh) * 100
                                : 0
                        };
                    })
                    .OrderBy(x => x.period)
                    .ToList();

                var totalPeak = dailyData.Sum(x => x.peakKwh);
                var totalNonPeak = dailyData.Sum(x => x.nonPeakKwh);
                var total = totalPeak + totalNonPeak;

                var response = new PeakNonPeakSummaryResponseDTO
                {
                    totalPeakKwh = totalPeak,
                    totalNonPeakKwh = totalNonPeak,
                    totalKwh = total,
                    peakPercentage = total > 0
                        ? (totalPeak / total) * 100
                        : 0,
                    nonPeakPercentage = total > 0
                        ? (totalNonPeak / total) * 100
                        : 0,
                    dailyData = dailyData
                };

                return new ResponseModel<PeakNonPeakSummaryResponseDTO>
                {
                    data = response,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<PeakNonPeakSummaryResponseDTO>
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }
        public async Task<byte[]> ExportPeakNonPeakAnalysisCsv(DateTime startDate, DateTime endDate)
        {
            var response = await GetPeakNonPeakAnalysis(startDate, endDate);
            var csv = new StringBuilder();

            csv.AppendLine("Date,Peak kWh,Non Peak kWh,Total kWh,Peak %,Non Peak %");

            foreach (var item in response.data?.dailyData ?? new())
            {
                csv.AppendLine($"{item.period},{item.peakKwh},{item.nonPeakKwh},{item.totalKwh},{item.peakPercentage},{item.nonPeakPercentage}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportEnergyConsumptionByDeviceTypeCsv()
        {
            var response = await GetEnergyConsumptionByDeviceTypeLast12Months();
            var csv = new StringBuilder();

            csv.AppendLine("Year,Month,Utility,Total kWh");

            foreach (var item in response.data ?? new())
            {
                csv.AppendLine($"{item.year},{item.month},{item.utilityName},{item.totalKwh}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private float CalculateConsumption<T>(List<T> rows) where T : class
        {
            if (rows == null || rows.Count < 2)
                return 0;

            dynamic first = rows.First();
            dynamic last = rows.Last();

            float consumption = last.active_energy - first.active_energy;

            return consumption < 0 ? 0 : consumption;
        }
    }
}