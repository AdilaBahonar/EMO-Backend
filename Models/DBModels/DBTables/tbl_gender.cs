using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_gender
    {
        [Key]
        public Guid gender_id { get; set; } = Guid.NewGuid();
        public string gender_name { get; set; } = string.Empty;
        public IEnumerable<tbl_user> users {get; set;} = default!;
    }
}
