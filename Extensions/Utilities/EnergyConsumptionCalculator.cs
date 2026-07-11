namespace EMO.Utilities
{
    public class EnergyCalculationResult
    {
        public double Consumption { get; set; }
        public int ResetCount { get; set; }
        public int IgnoredSpikeCount { get; set; }
    }

    public static class EnergyConsumptionCalculator
    {
        // Meter values are cumulative kWh. Correct usage is sensor-wise delta.
        // If a meter resets, current reading becomes smaller than previous.
        // In that case, count current value as consumption since reset instead of producing negative usage.
        public static EnergyCalculationResult CalculateSingleSensorConsumption<T>(
            IEnumerable<T> rows,
            Func<T, DateTime> timeSelector,
            Func<T, double> energySelector,
            double maxReasonableDeltaKwh = 1000)
        {
            var orderedRows = rows.OrderBy(timeSelector).ToList();
            var result = new EnergyCalculationResult();

            if (orderedRows.Count < 2) return result;

            for (var i = 1; i < orderedRows.Count; i++)
            {
                var previous = energySelector(orderedRows[i - 1]);
                var current = energySelector(orderedRows[i]);

                if (double.IsNaN(previous) || double.IsNaN(current) || current < 0) continue;

                double delta;

                if (current >= previous)
                {
                    delta = current - previous;
                }
                else
                {
                    // Reset detected. Example: previous=130.5, current=0.2.
                    // Count 0.2 kWh as the consumption after reset.
                    result.ResetCount++;
                    delta = current;
                }

                if (delta <= 0) continue;

                if (delta > maxReasonableDeltaKwh)
                {
                    result.IgnoredSpikeCount++;
                    continue;
                }

                result.Consumption += delta;
            }

            return result;
        }

        public static EnergyCalculationResult CalculateSensorWiseConsumption<T>(
            IEnumerable<T> rows,
            Func<T, Guid> sensorSelector,
            Func<T, DateTime> timeSelector,
            Func<T, double> energySelector,
            double maxReasonableDeltaKwh = 1000)
        {
            var result = new EnergyCalculationResult();

            foreach (var sensorRows in rows.GroupBy(sensorSelector))
            {
                var sensorResult = CalculateSingleSensorConsumption(
                    sensorRows,
                    timeSelector,
                    energySelector,
                    maxReasonableDeltaKwh);

                result.Consumption += sensorResult.Consumption;
                result.ResetCount += sensorResult.ResetCount;
                result.IgnoredSpikeCount += sensorResult.IgnoredSpikeCount;
            }

            return result;
        }
    }
}
