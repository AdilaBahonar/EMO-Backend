using System.ComponentModel.DataAnnotations;

namespace APIProduct.Models.DBModels.DBTables
{
    public class tbl_control_type
    {
        [Key]
        public Guid control_type_id { get; set; } = Guid.NewGuid();
        public string control_type_name { get; set; } = string.Empty;
        public IEnumerable<tbl_device> devices { get; set; } = default!;
    }
}
