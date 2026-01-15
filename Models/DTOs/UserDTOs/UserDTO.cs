using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.UserDTOs
{
    public class AddUserDTO
    {
        [Required]
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType {  get; set; } = string.Empty;


    }
    public  class UpdateInnerUserDTO
    {
        [Required]
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
    } 
    public  class UpdateUserDTO
    {
        [Required]
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
    }
    public class userResponseDTO 
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
    }
    public class UserInnerResponseDTO
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;
        public string userToken { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
        public string userTypeName {  get; set; } = string.Empty;
        public int userTypeLevel { get; set; } 
    }
    public class UserResponseDTO
    {
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string userPhone { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
        public string fkUserType { get; set; } = string.Empty;
        public string userTypeName {  get; set; } = string.Empty;
        public int userTypeLevel { get; set; }  
    }
}
