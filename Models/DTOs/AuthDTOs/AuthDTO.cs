using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.AuthDTOs
{
    public class UserLoginDTO
    {
        [Required]
        public string userName { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
        public bool isRememberMe = false;
    }
    public class UserAppLoginDTO
    {
        [Required]
        public string userPhoneNo { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
    }
    public class ForgotPasswordDTO
    {
        [Required]
        public string userName { get; set; } = string.Empty;
    }
    public class UserLoginResponseDTO
    {
        public string userId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public int userTypeLevel { get; set; } = 0;
        public int subUserTypeLevel { get; set; } = 0;
        public string fkGender { get; set; } = string.Empty;
        public string genderName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string imageBase64 { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string fkHandler { get; set; } = string.Empty;
        public string handlerName { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
    }
}
