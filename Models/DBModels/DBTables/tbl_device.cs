using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_device
    {
        [Key]
        public Guid device_id { get; set; } = Guid.NewGuid();
        public string device_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
