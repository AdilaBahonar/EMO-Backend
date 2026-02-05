using EMO.Models.DBModels.DBTables;
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_business
    {
        [Key]
        public Guid business_id { get; set; } = Guid.NewGuid();
        public string business_name { get; set; } = string.Empty;
        public string business_address { get; set; }= string.Empty;
        public string business_email { get; set; } = string.Empty;
        public string business_contact { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at {  get; set; } = DateTime.Now;
        public bool is_active { get; set; } = false;
        public IEnumerable<tbl_user> users { get; set; } = default!;
        public IEnumerable<tbl_facility> facilities { get; set; } = default!;
    }
}
