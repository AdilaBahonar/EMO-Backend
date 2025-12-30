using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_poc
    {
        [Key]
        public Guid poc_id { get; set; }= Guid.NewGuid();
        public string poc_name { get; set; } = string.Empty;
        public string poc_email { get; set; } = string.Empty;
        public string poc_phone_no { get; set; } = string.Empty;
    }
}
