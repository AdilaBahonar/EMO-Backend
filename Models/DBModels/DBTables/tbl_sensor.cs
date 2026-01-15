namespace EMO.Models.DBModels.DBTables
{
    public class tbl_sensor
    {
        public Guid sensor_id { get; set; } = Guid.NewGuid();
        public string sensor_name { get; set; } = string.Empty;
        public Guid fk_sensor_type { get; set; } = Guid.Empty;
        public tbl_sensor_type sensor_type { get; set; } = default! ;
        public Guid fk_office { get; set; } = Guid.Empty;
        public tbl_office office { get; set; } = default!;
        public Guid fk_device { get; set; } = Guid.Empty;
        public tbl_device device { get; set; } = default!;
        public Guid fk_utility { get; set; } = Guid.Empty;
        public tbl_utility utility { get; set; } = default!;
        public string mode_bus_address { get; set; } = string.Empty;
        public string meter_id {  get; set; } = string.Empty;
        public string serial_address {  get; set; } = string.Empty;
        public IEnumerable<tbl_singal_phase_data> singal_phase_data { get; set; } = default!;

    }
}
