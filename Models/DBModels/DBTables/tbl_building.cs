using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_building
    {
        [Key]
        public Guid building_id { get; set; } = Guid.NewGuid();
        public string building_name { get; set; } = string.Empty;
        public Guid fk_facility { get; set; } = Guid.Empty;
        public tbl_facility facility { get; set; } = default!;
        public IEnumerable<tbl_floor> floors { get; set; } = default!;
    }
}
