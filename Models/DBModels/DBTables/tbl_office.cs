using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_office
    {
        [Key]
        public Guid office_id { get; set; } = Guid.NewGuid();
        public string office_name { get; set; } = string.Empty;
        public Guid fk_section { get; set; } = Guid.Empty;
        public tbl_section section { get; set; } = default!;
        public IEnumerable<tbl_device> devices { get; set; } = default!;
    }
}
