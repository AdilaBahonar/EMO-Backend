using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_appliance
    {
        [Key]
        public Guid appliance_id { get; set; } = Guid.NewGuid();

        public string appliance_name { get; set; } = string.Empty;

        public string company_name { get; set; } = string.Empty;
        public string model_number { get; set; } = string.Empty;

        // Electrical expected values
        public float rated_voltage { get; set; } = 0;

        public float min_current { get; set; } = 0;
        public float max_current { get; set; } = 0;

        public float min_power { get; set; } = 0;
        public float max_power { get; set; } = 0;

        public float standby_power { get; set; } = 0;
        public float normal_power_factor { get; set; } = 0;

        public string description { get; set; } = string.Empty;

        // Optimization classification retained for safe prioritization.
        public string priority_level { get; set; } = "Normal";
        public bool is_critical { get; set; } = false;

        public bool is_default { get; set; } = false;
        public bool is_custom { get; set; } = false;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;

        public bool is_deleted { get; set; } = false;
        public bool is_active { get; set; } = true;

        // Relation with utility
        public Guid fk_utility { get; set; } = Guid.Empty;
        public tbl_utility utility { get; set; } = default!;

        // Business-owned copies created from this default appliance.
        public IEnumerable<tbl_business_appliance> business_appliances { get; set; } = default!;
    }
}

