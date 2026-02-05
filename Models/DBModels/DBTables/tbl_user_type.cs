using EMO.Models.DBModels.DBTables;
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_user_type
    {
        [Key]
        public Guid user_type_id { get; set; } = Guid.NewGuid();
        public string user_type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public int user_type_level { get; set; } 
        public IEnumerable<tbl_user> users { get; set; } = default!;
        public IEnumerable<tbl_sub_user_type> subuser_types { get; set; } = default!;
    }
}
