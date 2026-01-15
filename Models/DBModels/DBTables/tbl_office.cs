using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_office
    {
        [Key]
        public Guid office_id { get; set; } = Guid.NewGuid();
        public string office_name { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = null;
        public bool is_active { get; set; } = false;
        public bool is_occupied { get; set; } = false;
        public Guid fk_section { get; set; } = Guid.Empty;
        public tbl_section section { get; set; } = default!;
        public IEnumerable<tbl_sensor> sensors { get; set; } = default!;
        public IEnumerable<tbl_agreement> agreements { get; set; } = default!;
    }
}
