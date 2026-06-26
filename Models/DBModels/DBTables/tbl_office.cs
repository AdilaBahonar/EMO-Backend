using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DBModels.DBTables
{
    public class tbl_office
    {
        [Key]
        public Guid office_id { get; set; } = Guid.NewGuid();
        public string office_name { get; set; } = string.Empty;

        // Office working schedule - used for after-hours wastage / idle alerts
        public TimeOnly opening_time { get; set; } = new TimeOnly(9, 0);
        public TimeOnly closing_time { get; set; } = new TimeOnly(18, 0);
        public string working_days { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
        public bool is_24_hours { get; set; } = false;
        public bool after_hours_alert_enabled { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
        public bool is_deleted { get; set; } = false;
        public bool is_active { get; set; } = false;
        public bool is_occupied { get; set; } = false;

        public Guid fk_section { get; set; } = Guid.Empty;
        public Guid fk_business { get; set; } = Guid.Empty;

        public tbl_business business { get; set; } = default!;
        public tbl_section section { get; set; } = default!;
        public IEnumerable<tbl_device> devices { get; set; } = default!;
        public IEnumerable<tbl_office_agreement> office_agreement { get; set; } = default!;
    }
}
