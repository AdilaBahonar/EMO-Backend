namespace EMO.Models.DBModels.DBTables
{
    public class tbl_singal_phase_data
    {
        public Guid singal_phase_data_id { get; set; } =Guid.NewGuid();
        public Int32 packet_id { get; set; } = 0;
        public Int32 epoch_sec { get; set; } = 0;
        public float volt { get; set; } = 0;
        public float current { get; set; } = 0;
        public float apperent_power { get; set; } = 0;
        public float active_power { get; set; } = 0;
        public float reactive_power { get; set; } = 0;
        public float power_factor { get; set; } = 0;
        public float frequency { get; set; } = 0;
        public float active_energy { get; set; } = 0;
        public float reactive_energy { get; set; } = 0;
        public DateTime created_at { get; set; } = DateTime.Now;
        public Guid fk_sensor { get; set; } = Guid.Empty;
        public tbl_sensor sensor { get; set; } = default!;
    }
}
