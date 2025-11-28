using APIProduct.Models.DBModels.DBTables;
using P3AHR.Extensions;
using System.ComponentModel.DataAnnotations;

namespace P3AHR.Models.DBModels.DBTables
{
    public class tbl_user
    {
        [Key]
        public Guid user_id { get; set; } = Guid.NewGuid();
        public string user_name { get; set; } = string.Empty;
        public string? user_persaonal_email { get; set; } = null;
        public string? user_official_email { get; set; } = null;
        public string user_phone_no { get; set; } = string.Empty;
        public string user_token { get; set; } = string.Empty;
        public string user_password { get; set; } = new OtherServices().encodePassword("12345");
        public Guid? fk_user_type { get; set; } = Guid.Empty;
        public tbl_user_type user_type { get; set; } = default!;
        //public Guid fk_user_type { get; set; } = default!;
        //public tbl_user_type user_type { get; set; } = default!;
        //public tbl_user_designation user_designation { get; set; } = default!;
    }
}
