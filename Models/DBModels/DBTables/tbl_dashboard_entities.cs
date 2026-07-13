//using System.ComponentModel.DataAnnotations;

//namespace EMO.Models.DBModels.DBTables
//{
//    public class tbl_business_dashboard_setting
//    {
//        [Key]
//        public Guid business_dashboard_setting_id { get; set; } = Guid.NewGuid();
//        public Guid fk_business { get; set; } = Guid.Empty;
//        public string peak_start_time { get; set; } = "18:00";
//        public string peak_end_time { get; set; } = "23:00";
//        public int? day_of_week { get; set; } = null;
//        public string timezone { get; set; } = "Asia/Karachi";
//        public string currency { get; set; } = "PKR";
//        public decimal tariff_rate { get; set; } = 65;
//        public decimal peak_tariff_rate { get; set; } = 75;
//        public decimal off_peak_tariff_rate { get; set; } = 55;
//        public int online_sensor_threshold_seconds { get; set; } = 60;
//        public bool is_active { get; set; } = true;
//        public bool is_deleted { get; set; } = false;
//        public DateTime created_at { get; set; } = DateTime.UtcNow;
//        public DateTime updated_at { get; set; } = DateTime.UtcNow;
//    }

//    public class tbl_dashboard_aggregate
//    {
//        [Key]
//        public Guid dashboard_aggregate_id { get; set; } = Guid.NewGuid();
//        public Guid fk_business { get; set; } = Guid.Empty;
//        public Guid? fk_tenant { get; set; }
//        public Guid? fk_office { get; set; }
//        public DateTime from_time { get; set; }
//        public DateTime to_time { get; set; }
//        public string granularity { get; set; } = "custom";
//        public double total_energy_kwh { get; set; }
//        public double current_load_w { get; set; }
//        public double estimated_cost { get; set; }
//        public int online_sensors { get; set; }
//        public double saving_opportunity { get; set; }
//        public double peak_kwh { get; set; }
//        public double offpeak_kwh { get; set; }
//        public double peak_demand_w { get; set; }
//        public DateTime created_at { get; set; } = DateTime.UtcNow;
//        public DateTime updated_at { get; set; } = DateTime.UtcNow;
//    }

//    public class tbl_dashboard_chart_aggregate
//    {
//        [Key]
//        public Guid dashboard_chart_aggregate_id { get; set; } = Guid.NewGuid();
//        public Guid fk_business { get; set; } = Guid.Empty;
//        public Guid? fk_tenant { get; set; }
//        public string chart_type { get; set; } = string.Empty;
//        public string range_key { get; set; } = string.Empty;
//        public DateTime from_time { get; set; }
//        public DateTime to_time { get; set; }
//        public string payload_json { get; set; } = string.Empty;
//        public DateTime created_at { get; set; } = DateTime.UtcNow;
//        public DateTime updated_at { get; set; } = DateTime.UtcNow;
//    }

//    public class tbl_dashboard_suggestion
//    {
//        [Key]
//        public Guid dashboard_suggestion_id { get; set; } = Guid.NewGuid();
//        public Guid fk_business { get; set; } = Guid.Empty;
//        public Guid? fk_tenant { get; set; }
//        public Guid? fk_sensor { get; set; }
//        public Guid? fk_office { get; set; }
//        public Guid? fk_appliance { get; set; }
//        public DateTime from_time { get; set; }
//        public DateTime to_time { get; set; }
//        public string type { get; set; } = "general";
//        public string severity { get; set; } = "info";
//        public string title { get; set; } = string.Empty;
//        public string description { get; set; } = string.Empty;
//        public string action { get; set; } = string.Empty;
//        public string priority { get; set; } = "Low";
//        public double? estimated_saving_kwh { get; set; }
//        public double? estimated_saving_cost { get; set; }
//        public string affected_appliance { get; set; } = string.Empty;
//        public string affected_utility { get; set; } = string.Empty;
//        public string affected_office { get; set; } = string.Empty;
//        public string recommendation { get; set; } = string.Empty;
//        public string reason { get; set; } = string.Empty;
//        public string reason_code { get; set; } = string.Empty;
//        public string confidence { get; set; } = "Low";
//        public bool conflicts_with_peak_hour { get; set; }
//        public bool can_apply_action { get; set; } = false;
//        public DateTime created_at { get; set; } = DateTime.UtcNow;
//        public DateTime updated_at { get; set; } = DateTime.UtcNow;
//    }
//}
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_business_dashboard_setting
    {
        [Key]
        public Guid business_dashboard_setting_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; } = Guid.Empty;
        public string peak_start_time { get; set; } = "18:00";
        public string peak_end_time { get; set; } = "23:00";
        public int? day_of_week { get; set; } = null;
        public string timezone { get; set; } = "Asia/Karachi";
        public string currency { get; set; } = "PKR";
        public decimal tariff_rate { get; set; } = 65;
        public decimal peak_tariff_rate { get; set; } = 75;
        public decimal off_peak_tariff_rate { get; set; } = 55;
        public int online_sensor_threshold_seconds { get; set; } = 60;
        public bool is_active { get; set; } = true;
        public bool is_deleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class tbl_dashboard_aggregate
    {
        [Key]
        public Guid dashboard_aggregate_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; } = Guid.Empty;
        public Guid? fk_tenant { get; set; }
        public Guid? fk_office { get; set; }
        public DateTime from_time { get; set; }
        public DateTime to_time { get; set; }
        public string granularity { get; set; } = "custom";
        public double total_energy_kwh { get; set; }
        public double current_load_w { get; set; }
        public double estimated_cost { get; set; }
        public int online_sensors { get; set; }
        public double saving_opportunity { get; set; }
        public double peak_kwh { get; set; }
        public double offpeak_kwh { get; set; }
        public double peak_demand_w { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    [Index(nameof(scope_level), nameof(scope_id), nameof(chart_type), nameof(range_key), Name = "UX_dashboard_chart_scope", IsUnique = true)]
    public class tbl_dashboard_chart_aggregate
    {
        [Key]
        public Guid dashboard_chart_aggregate_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; } = Guid.Empty;
        public Guid? fk_tenant { get; set; }
        public string? scope_level { get; set; }
        public Guid? scope_id { get; set; }
        public string chart_type { get; set; } = string.Empty;
        public string range_key { get; set; } = string.Empty;
        public DateTime from_time { get; set; }
        public DateTime to_time { get; set; }
        public string payload_json { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }

    public class tbl_dashboard_suggestion
    {
        [Key]
        public Guid dashboard_suggestion_id { get; set; } = Guid.NewGuid();
        public Guid fk_business { get; set; } = Guid.Empty;
        public Guid? fk_tenant { get; set; }
        public Guid? fk_sensor { get; set; }
        public Guid? fk_office { get; set; }
        public Guid? fk_appliance { get; set; }
        public DateTime from_time { get; set; }
        public DateTime to_time { get; set; }
        public string type { get; set; } = "general";
        public string severity { get; set; } = "info";
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string priority { get; set; } = "Low";
        public double? estimated_saving_kwh { get; set; }
        public double? estimated_saving_cost { get; set; }
        public string affected_appliance { get; set; } = string.Empty;
        public string affected_utility { get; set; } = string.Empty;
        public string affected_office { get; set; } = string.Empty;
        public string recommendation { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
        public string reason_code { get; set; } = string.Empty;
        public string confidence { get; set; } = "Low";
        public bool conflicts_with_peak_hour { get; set; }
        public bool can_apply_action { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}
