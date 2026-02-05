using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.UserTypeDTOs
{
    public class AddUserTypeDTO
    {
        public string userTypeName { get; set; } = string.Empty;
        public int userTypeLevel { get; set; }
        public bool isActive { get; set; } = true;
       
    }
    public class UpdateUserTypeDTO
    {
        [Required]
        public string userTypeId { get; set; } = string.Empty;
        public string userTypeName { get; set; } = string.Empty;
        public int userTypeLevel { get; set; }
        public bool isActive { get; set; } 

    }
    public class UserTypeResponseDTO
    {
        public string userTypeId { get; set; } = string.Empty;
        public string userTypeName { get; set; } = string.Empty;
        public int userTypeLevel { get; set; }
        public bool isActive { get; set; } 

    }
    public class UserTypeHierarchyDTO
    {
        [Required]
        public string userTypeId { get; set; } = string.Empty;
        [Required]
        public int userTypeLevel { get; set; }

    }
}
