using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_device
    {
        [Key]
        public Guid device_id { get; set; } = Guid.NewGuid();
        public string device_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public Guid fk_business { get; set; } = Guid.Empty;
        
        //public Guid? fk_office { get; set; } = null;
        //public tbl_office office { get; set; } = default!;

        public tbl_business business { get; set; } = default!;
        public IEnumerable<tbl_sensor> sensors { get; set; } = default!;
    }
}
