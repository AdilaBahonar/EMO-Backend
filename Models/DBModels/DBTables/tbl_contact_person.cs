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
        public Guid fk_agreement {  get; set; } =Guid.Empty;
        public tbl_agreement agreement { get; set; } = default!;
        public bool is_deleted { get; set; } = false;
    }
}
