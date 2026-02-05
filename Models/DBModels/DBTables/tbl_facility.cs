using EMO.Models.DBModels.DBTables;
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_facility
    {
        [Key]
        public Guid facility_id { get; set; } = Guid.NewGuid();
        public string facility_name { get; set; } = string.Empty;
        public string facility_address { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
        public bool is_active { get; set; } = false;
        public Guid fk_business { get; set; } = Guid.Empty;
        public tbl_business business { get; set; } = default!;
        public IEnumerable<tbl_building> buildings { get; set; } = default!;
    }
}
