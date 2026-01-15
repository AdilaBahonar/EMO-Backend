using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.OfficeDTOs
{
    public class AddOfficeDTO
    {
        [Required]
        public string officeName { get; set; } = string.Empty;
        [Required]
        public string fkSection { get; set; } = string.Empty;
    }
    public class UpdateOfficeDTO
    {
        [Required]
        public string officeId { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string fkSection { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
    public class OfficeResponseDTO
    {
        [Required]
        public string officeId { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string fkSection { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
