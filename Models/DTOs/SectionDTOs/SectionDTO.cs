using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SectionDTOs
{
    public class AddSectionDTO
    {
        [Required]
        public string sectionName { get; set; } = string.Empty;
        [Required]
        public string fkFloor { get; set; } = string.Empty;
        [Required]
        public string fkBusiness { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
    public class UpdateSectionDTO
    {
        [Required]
        public string sectionId { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string fkFloor { get; set; } = string.Empty; 
        public bool isActive { get; set; } = false;
    }
    public class SectionResponseDTO
    {
        [Required]
        public string sectionId { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
        public string fkFloor { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
}
