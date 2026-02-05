using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_sub_user_type
    {
        [Key]
        public Guid sub_user_type_id { get; set; } = Guid.NewGuid();
        public string sub_user_type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public int sub_user_type_level { get; set; }
        public Guid fk_user_type { get; set; } = Guid.Empty;
        public tbl_user_type user_type { get; set; } = default!;
        public IEnumerable<tbl_user> users { get; set; } = default!;

    }
}
