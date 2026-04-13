using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_office_agreement
    {
        [Key]
        public Guid office_agreement_id { get; set; } = Guid.NewGuid();
        public Guid fk_agreement { get; set; } = Guid.Empty;
        public tbl_agreement agreement { get; set; } = default!;
        public Guid fk_office { get; set; } = Guid.Empty;

        public tbl_office office { get; set; } = default!;
    }
}
