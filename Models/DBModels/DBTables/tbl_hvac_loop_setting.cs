using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_hvac_loop_setting
    {
        [Key]
        public Guid hvac_loop_setting_id { get; set; } = Guid.NewGuid();

        public Guid fk_sensor { get; set; } = Guid.Empty;
        public tbl_sensor sensor { get; set; } = default!;

        public bool loop_enabled { get; set; } = false;

        public int loop_on_seconds { get; set; } = 0;
        public int loop_off_seconds { get; set; } = 0;

        // Exact loop start moment. Use UTC when setting this value.
        public DateTime? loop_started_at { get; set; } = null;

        public bool is_active { get; set; } = true;
        public bool is_deleted { get; set; } = false;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
    }
}
