using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_business_appliance
    {
        [Key]
        public Guid business_appliance_id { get; set; } = Guid.NewGuid();

        public Guid fk_business { get; set; } = Guid.Empty;
        public tbl_business business { get; set; } = default!;

        public Guid? fk_appliance { get; set; }
        public tbl_appliance? default_appliance { get; set; }

        public string appliance_name { get; set; } = string.Empty;
        public string company_name { get; set; } = string.Empty;
        public string model_number { get; set; } = string.Empty;

        public float rated_voltage { get; set; } = 0;

        public float min_current { get; set; } = 0;
        public float max_current { get; set; } = 0;

        public float min_power { get; set; } = 0;
        public float max_power { get; set; } = 0;

        public float standby_power { get; set; } = 0;
        public float normal_power_factor { get; set; } = 0;

        public string description { get; set; } = string.Empty;

        public bool is_shiftable { get; set; } = false;
        public string priority_level { get; set; } = "Normal";
        public string normal_operating_hours { get; set; } = string.Empty;
        public bool can_auto_control { get; set; } = false;
        public bool is_critical { get; set; } = false;
        public bool allow_optimization_suggestions { get; set; } = true;
        public string allowed_shift_start_time { get; set; } = string.Empty;
        public string allowed_shift_end_time { get; set; } = string.Empty;
        public int minimum_on_duration_minutes { get; set; } = 0;
        public int minimum_off_duration_minutes { get; set; } = 0;

        public bool is_default { get; set; } = false;
        public bool is_custom { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;

        public bool is_deleted { get; set; } = false;
        public bool is_active { get; set; } = true;

        public Guid fk_utility { get; set; } = Guid.Empty;
        public tbl_utility utility { get; set; } = default!;

        public IEnumerable<tbl_sensor_appliance> sensor_appliances { get; set; } = default!;
    }
}
