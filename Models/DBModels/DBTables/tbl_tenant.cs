namespace EMO.Models.DBModels.DBTables
{
    public class tbl_tenant
    {
        public Guid tenant_id { get; set; } = Guid.NewGuid();
        public string tenant_name { get; set; } = string.Empty;
        public string tenant_ntn { get; set;} = string.Empty;
        public string tenant_address { get; set;} = string.Empty;
        public string tenant_coin { get; set;} = string.Empty;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = null;
        public bool is_active { get; set; } = false;

    }
}
