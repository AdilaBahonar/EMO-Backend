using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_agreement
    {
        [Key]
        public Guid agreement_id { get; set; } = Guid.NewGuid();
        public string agreement_name { get; set; } = string.Empty;
        public string agreement_description { get; set; } = string.Empty;
        public DateTime agreement_start_date { get; set; } = DateTime.Now;
        public DateTime agreement_end_date { get; set; } = DateTime.Now.AddYears(1);
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = null;
        public bool is_active { get; set; } = false;
        public Guid fk_tenant { get; set; } = Guid.Empty;
        public tbl_tenant tenant { get; set; } = default!;
        public Guid fk_office { get; set; } = Guid.Empty;
        public tbl_office office { get; set; } = default!;
    }
}
