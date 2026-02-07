using EMO.Models.DBModels.DBTables;
using EMO.Extensions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.IdentityModel.Tokens;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_user
    {
        //[Key]
        //public Guid user_id { get; set; } = Guid.NewGuid();
        //public string user_name { get; set; } = string.Empty;
        //public string otp { get; set; } = string.Empty;
        //public string name { get; set; } = string.Empty;
        //public string user_phone_no { get; set; } = string.Empty;
        //public string user_token { get; set; } = string.Empty;
        //public string user_password { get; set; } = new OtherServices().encodePassword("12345");
        //public Guid fk_gender { get; set; } = Guid.Empty;
        //public tbl_gender gender { get; set; } = default!;
        //public Guid? fk_subuser_type { get; set; } = Guid.Empty;
        //public tbl_subuser_type subuser_type { get; set; } = default!;
        //
        //public Guid fk_user_type { get; set; } = default!;
        //public tbl_user_type user_type { get; set; } = default!;
        //public tbl_user_designation user_designation { get; set; } = default!;


        [Key]
        public Guid user_id { get; set; } = Guid.NewGuid();
        public string name { get; set; } = string.Empty;
        public string user_name { get; set; } = null!;
        public string otp { get; set; } = string.Empty;
        public string user_email { get; set; } = string.Empty;
        public string user_phone_no { get; set; } = string.Empty;
        public string user_token { get; set; } = string.Empty;
        public Guid? fk_business { get; set; } = null;
        public Guid? fk_handler{ get; set; } = null;
        public tbl_business businesses { get; set; } = default!;
        public tbl_user handler { get; set; } = default!;
        public DateTime? last_activity_at { get; set; } 
        public string user_password { get; set; } = new OtherServices().encodePassword("12345");
        public bool is_active { get; set; } = true;
        public Guid? fk_gender { get; set; } = null;
        public tbl_gender? gender { get; set; } = null;
        public Guid? fk_sub_user_type { get; set; } = Guid.Empty;
        public tbl_sub_user_type sub_user_type { get; set; } = default!;
        public tbl_user_image user_image { get; set; } = default!;
        public IEnumerable<tbl_user> users { get; set; } = default!;
        public IEnumerable<tbl_contact_person> contact_persons { get; set; } = default!;
        public IEnumerable<tbl_agreement> agreements { get; set; } = default!;
    }
}
