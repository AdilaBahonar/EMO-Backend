using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_singal_phase_data
    {
        [Key]
        public Guid singal_phase_data_id { get; set; } =Guid.NewGuid();
        public Int32 packet_id { get; set; } = 0;
        public Int32 epoch_sec { get; set; } = 0;
        public float volt { get; set; } = 0;
        public float current { get; set; } = 0;
        public float apperent_power { get; set; } = 0;
        public float active_power { get; set; } = 0;
        public float reactive_power { get; set; } = 0;
        public float power_factor { get; set; } = 0;
        public float frequency { get; set; } = 0;
        public float active_energy { get; set; } = 0;
        public float reactive_energy { get; set; } = 0;
        public DateTime created_at { get; set; } = DateTime.Now;
        public Guid fk_sensor { get; set; } = Guid.Empty;
        public tbl_sensor sensor { get; set; } = default!;
        public bool is_deleted { get; set; } = false;
    }
    public class tbl_daily_energy
    {
        [Key]
        public Guid daily_energy_id { get; set; } = Guid.NewGuid();

        public Guid fk_sensor { get; set; }

        public DateTime date { get; set; } // ONLY DATE (no time)

        // ================= ENERGY =================
        public double total_active_energy { get; set; }
        public double total_reactive_energy { get; set; }

        // ================= POWER =================
        public double avg_active_power { get; set; }
        public double max_active_power { get; set; }
        public double min_active_power { get; set; }

        public double avg_voltage { get; set; }
        public double avg_current { get; set; }
        public double avg_power_factor { get; set; }

        // ================= META =================
        public int sample_count { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
    }

    public class tbl_monthly_energy
    {
        [Key]
        public Guid monthly_energy_id { get; set; } = Guid.NewGuid();

        public Guid fk_sensor { get; set; }

        public int year { get; set; }
        public int month { get; set; }

        // ================= ENERGY =================
        public double total_active_energy { get; set; }
        public double total_reactive_energy { get; set; }

        // ================= POWER =================
        public double avg_active_power { get; set; }
        public double max_active_power { get; set; }

        public double avg_voltage { get; set; }
        public double avg_current { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
    }

}
