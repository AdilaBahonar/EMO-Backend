using P3AHR.Models.DTOs.ResponseDTO;

namespace P3AHR.Models.DTOs.LoginDTOs
{
    public class LoginResponseDTO : ResponseModel
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string userPhone { get; set; } = string.Empty;
        public string userOfficialEmail {  get; set; } = string.Empty;
    }
    public class LoginRequestDTO
    {
        public string userId { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
        public string userPhone { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;    
        public bool userRemember { get; set; } = false; 
        public string userOfficialEmail { get; set; } = string.Empty;
    }
}
