using System.ComponentModel.DataAnnotations;

namespace APIProduct.Models.DBModels.DBTables
{
    public class tbl_section
    {
        [Key]
        public Guid section_id { get; set; } = Guid.NewGuid();
        public string section_name { get; set; } = string.Empty;
        public Guid fk_floor { get; set; } = Guid.Empty;
        public tbl_floor floor { get; set; } = default!;
        public IEnumerable<tbl_office> offices { get; set; } = default!;
    }
}
