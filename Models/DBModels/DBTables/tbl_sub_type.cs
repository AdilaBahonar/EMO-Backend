namespace EMO.Models.DBModels.DBTables
{
    public class tbl_sub_type
    {
        public Guid sub_type_id {  get; set; } =Guid.NewGuid();
        public string sub_type_name { get; set; } = string.Empty;
        public string sub_type_level { get; set; } = string.Empty;
        public Guid fk_user_type { get; set; } = Guid.Empty;
        public tbl_user-type user_type { get; set; } = default; 
    }
}
