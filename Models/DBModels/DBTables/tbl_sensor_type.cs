using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_sensor_type
    {
        [Key]
        public Guid sensor_type_id { get; set; } = Guid.NewGuid();
        public string sensor_type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public int is_type { get; set; } = 0;/*
        public IEnumerable<tbl_sensor> sensors { get; set; } = default!;*/
    }
}
