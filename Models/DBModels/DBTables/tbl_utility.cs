namespace EMO.Models.DBModels.DBTables
{
    public class tbl_utility
    {
        public Guid utility_id { get; set; } =Guid.NewGuid();
        public string utility_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
