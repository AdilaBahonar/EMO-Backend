using Microsoft.EntityFrameworkCore;

namespace EMO.Models.DBModels.DBTables
{
    /// <summary>
    /// Rebuildable 15-minute sensor aggregate used by dashboard and Deep Dive APIs.
    /// The original minute-level readings in tbl_singal_phase_data remain the source of truth.
    /// </summary>
    [Index(nameof(bucket_start), nameof(fk_sensor), Name = "ix_sensor_energy_15min_bucket")]
    public class tbl_sensor_energy_15min
    {
        public Guid fk_sensor { get; set; }
        public DateTime bucket_start { get; set; }

        public double energy_kwh { get; set; }
        public double reactive_energy_kvarh { get; set; }

        public double avg_active_power_w { get; set; }
        public double max_active_power_w { get; set; }
        public double avg_voltage_v { get; set; }
        public double avg_current_a { get; set; }
        public double avg_reactive_power_var { get; set; }
        public double avg_apparent_power_va { get; set; }
        public double avg_power_factor { get; set; }
        public double avg_frequency_hz { get; set; }

        public int sample_count { get; set; }
        public int pf_excellent_count { get; set; }
        public int pf_good_count { get; set; }
        public int pf_acceptable_count { get; set; }
        public int pf_poor_count { get; set; }
        public int alert_sample_count { get; set; }

        public DateTime first_reading_at { get; set; }
        public DateTime last_reading_at { get; set; }

        public int reset_count { get; set; }
        public int ignored_spike_count { get; set; }
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}
