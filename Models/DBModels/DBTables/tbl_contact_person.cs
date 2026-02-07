using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_contact_person
    {
        [Key]
        public Guid contact_person_id { get; set; } = Guid.NewGuid();
        public string contact_person_name { get; set; } = string.Empty;
        public string contact_person_email { get; set; } = string.Empty;
        public string contact_person_phone { get; set; } = string.Empty;
        public Guid fk_tenant {  get; set; } =Guid.Empty;
        public tbl_user tenant { get; set; } = default!;
    }
}
