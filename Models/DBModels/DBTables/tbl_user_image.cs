
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_user_image
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid fk_user { get; set; }

        [Required]
        public string imageBase64 { get; set; } = default!;
        public tbl_user? user { get; set; } = null;
    }

}
