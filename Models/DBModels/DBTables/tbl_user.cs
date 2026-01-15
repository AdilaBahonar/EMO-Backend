using EMO.Models.DBModels.DBTables;
using EMO.Extensions;
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_user
    {
        [Key]
        public Guid user_id { get; set; } = Guid.NewGuid();
        public string user_name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string user_phone_no { get; set; } = string.Empty;
        public string user_token { get; set; } = string.Empty;
        public Guid? fk_user_type { get; set; } = Guid.Empty;
        public tbl_user_type user_type { get; set; } = default!;
        public IEnumerable<tbl_business> businesses { get; set; } = default!;
        //public Guid fk_user_type { get; set; } = default!;
        //public tbl_user_type user_type { get; set; } = default!;
        //public tbl_user_designation user_designation { get; set; } = default!;
    }
}
