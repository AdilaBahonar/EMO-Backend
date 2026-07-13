using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_energy_tariff_plan
    {
        [Key]
        public Guid energy_tariff_plan_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; }
        public string plan_name { get; set; } = "Default Energy Tariff";
        public string currency { get; set; } = "PKR";
        public decimal standard_rate_per_kwh { get; set; }
        public decimal peak_rate_per_kwh { get; set; }
        public decimal off_peak_rate_per_kwh { get; set; }
        public decimal? demand_charge_per_kw { get; set; }
        public bool is_active { get; set; } = true;
        public bool is_deleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class tbl_tariff_time_period
    {
        [Key]
        public Guid tariff_time_period_id { get; set; } = Guid.NewGuid();
        public Guid fk_tariff_plan { get; set; }
        public string period_name { get; set; } = "Peak";
        public string period_type { get; set; } = "Peak";
        public TimeOnly start_time { get; set; }
        public TimeOnly end_time { get; set; }
        public int? day_of_week { get; set; }
        public DateOnly? season_start { get; set; }
        public DateOnly? season_end { get; set; }
        public bool is_active { get; set; } = true;
        public bool is_deleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class tbl_demand_management_setting
    {
        [Key]
        public Guid demand_management_setting_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; }
        public decimal demand_limit_kw { get; set; } = 15;
        public decimal warning_threshold_percent { get; set; } = 90;
        public decimal recovery_threshold_kw { get; set; } = 13.5m;
        public int demand_interval_minutes { get; set; } = 15;
        public int stabilization_minutes { get; set; } = 5;
        public bool enable_peak_hour_control { get; set; }
        public bool enable_demand_threshold_control { get; set; }
        public bool suggestion_only_mode { get; set; } = true;
        public bool is_active { get; set; } = true;
        public bool is_deleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}
