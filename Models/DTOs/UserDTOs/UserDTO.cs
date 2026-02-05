using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.UserDTOs
{
    public class AddUserDTO
    {
        [Required]
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string? fkBusiness { get; set; } = null;
        public string userPassword { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public bool isActive { get; set; } = true;
        public Guid fkGender { get; set; } = Guid.Empty;
        public string? fkHandler { get; set; } = null;
        public string? imageBase64 { get; set; } = null;
    }
    public  class UpdateInnerUserDTO
    {
        [Required]
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string lastActivityAt { get; set; } = string.Empty;
        public int userTypeLevel { get; set; } = 0;
        public int subUserTypeLevel { get; set; } = 0;
        public string? fkHandler { get; set; } = null;
        public string handlerName { get; set; } = string.Empty;
        public string? fkBusiness { get; set; } = null;
        public string businessName { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
    } 
    public  class UpdateUserDTO
    {
        [Required]
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public bool isActive { get; set; }
        public string? fkGender { get; set; } = string.Empty;
        public bool isImageChanged { get; set; } = false;
        public string? imageBase64 { get; set; } = null;
    }
    public class userResponseDTO 
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
    }
    public class UserInnerResponseDTO
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public DateTime lastActivityAt { get; set; }
        public string fkSubUserType { get; set; } = string.Empty;
        public string subUserTypeName { get; set; } = string.Empty;
        public int userTypeLevel { get; set; } = 0;
        public int subUserTypeLevel { get; set; } = 0;
        public Guid? fkGender { get; set; } = null;
        public string genderName { get; set; } = string.Empty;
        public string? fkHandler { get; set; } = null;
        public string handlerName { get; set; } = string.Empty;
        public string? fkBusiness { get; set; } = null;
        public string businessName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string? imageBase64 { get; set; } = null;
    }
    public class UserResponseDTO
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userPhone { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public string subUserTypeName { get; set; } = string.Empty;
        public int subUserTypeLevel { get; set; } = 0;
        public string? fkBusiness { get; set; } = null;
        public string businessName { get; set; } = string.Empty;
        public string? fkHandler{ get; set; } = null;
        public string handlerName { get; set; } = string.Empty;
        public Guid? fkGender { get; set; } = null;
        public string genderName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string? imageBase64 { get; set; } = null;
    }
}
