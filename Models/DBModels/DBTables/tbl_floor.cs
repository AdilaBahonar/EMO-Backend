using System.ComponentModel.DataAnnotations;

namespace APIProduct.Models.DBModels.DBTables
{
    public class tbl_floor
    {
        [Key]
        public Guid floor_id { get; set; } = Guid.NewGuid();
        public string floor_name { get; set; } = string.Empty;
        public int floor_no { get; set; } = 0; 
        public Guid fk_building { get; set; } = Guid.Empty;
        public tbl_building building { get; set; } = default!;
        public IEnumerable<tbl_section> sections { get; set; } = default!;
    }
}
