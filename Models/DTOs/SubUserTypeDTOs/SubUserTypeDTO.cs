using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SubUserTypeDTOs
{
    public class AddSubUserTypeDTO
    {
        public string subUserTypeName { get; set; } = string.Empty;
        public int subUserTypeLevel { get; set; } = 0;
        public string fkUserTypeId { get; set; } = string.Empty;
        public bool isActive { get; set; } = true;

    }
    public class UpdateSubUserTypeDTO
    {
        [Required]
        public string subUserTypeId { get; set; } = string.Empty;
        public string subUserTypeName { get; set; } = string.Empty;
        public string fkUserTypeId { get; set; } = string.Empty;
        public int subUserTypeLevel { get; set; } = 0;
        public bool isActive { get; set; } = false;

    }
    public class SubUserTypeResponseDTO
    {
        public string subUserTypeId { get; set; } = string.Empty;
        public string subUserTypeName { get; set; } = string.Empty;
        public string fkUserTypeId { get; set; } = string.Empty;
        public string userTypeName { get; set; } = string.Empty;
        public int userTypeLevel { get; set; } = 0;
        public int subUserTypeLevel { get; set; } = 0;
        public bool isActive { get; set; } = false;

    }
    public class SubUserTypeHierarchyDTO
    {
        [Required]
        public string subUserTypeId { get; set; } = string.Empty;
        [Required]
        public int subUserTypeLevel { get; set; } = 0;

    }
}
