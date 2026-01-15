using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SectionDTOs
{
    public class AddSectionDTO
    {
        [Required]
        public string sectionName { get; set; } = string.Empty;
        [Required]
        public string fkFloor { get; set; } = string.Empty;
    }
    public class UpdateSectionDTO
    {
        [Required]
        public string sectionId { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
        public string fkFloor { get; set; } = string.Empty; 
        public bool is_active { get; set; } = false;
    }
    public class SectionResponseDTO
    {
        [Required]
        public string sectionId { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;
        public string fkFloor { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
