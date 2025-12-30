using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_device
    {
        [Key]
        public Guid device_id { get; set; } = Guid.NewGuid();
        public string device_name { get; set; } = string.Empty;
        public Guid fk_office { get; set; } = Guid.Empty;
        public tbl_office office { get; set; } = default!;
        public Guid fk_control_type { get; set; } = Guid.Empty;
        public tbl_control_type control_type { get; set; } = default!;
        public Guid fk_device_type { get; set; } = Guid.Empty;
        public tbl_device_type device_type { get; set; } = default!;
    }
}
