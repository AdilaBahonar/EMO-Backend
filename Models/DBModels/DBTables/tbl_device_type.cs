using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_device_type
    {
        [Key]
        public Guid device_type_id { get; set; } = Guid.NewGuid();
        public string device_type_name { get; set; } = string.Empty;
        public IEnumerable<tbl_device> devices { get; set; } = default!;
    }
}
