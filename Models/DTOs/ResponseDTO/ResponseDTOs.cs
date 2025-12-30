namespace EMO.Models.DTOs.ResponseDTO
{
    public class ResponseModel
    {
        public string remarks { get; set; } = string.Empty;
        public bool success { get; set; } = false;
    }
    public class ResponseModel<anyType>
    {
        public anyType data { get; set; } = default!;
        public string remarks { get; set; } = string.Empty;
        public bool success { get; set; } = false;
    }
}
