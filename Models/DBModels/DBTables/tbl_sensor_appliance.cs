using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_sensor_appliance
    {
        [Key]
        public Guid sensor_appliance_id { get; set; } = Guid.NewGuid();

        // Sensor relation
        public Guid fk_sensor { get; set; } = Guid.Empty;
        public tbl_sensor sensor { get; set; } = default!;

        // Appliance relation
        public Guid fk_appliance { get; set; } = Guid.Empty;
        public tbl_business_appliance appliance { get; set; } = default!;

        // Optional fields
        public string remarks { get; set; } = string.Empty;
        public DateTime assigned_at { get; set; } = DateTime.Now;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;

        public bool is_deleted { get; set; } = false;
        public bool is_active { get; set; } = true;
    }
}
