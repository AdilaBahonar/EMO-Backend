using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.FacilityDTOs
{
    public class AddFacilityDTO
    {
        [Required]
        public string facilityName { get; set; } = string.Empty;
        [Required]
        public string fkBusiness { get; set; } = string.Empty;
    }
    public class UpdateFacilityDTO
    {
        [Required]
        public string facilityId { get; set; } = string.Empty;
        public string facilityName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
    public class FacilityResponseDTO
    {
        [Required]
        public string facilityId { get; set; } = string.Empty;
        public string facilityName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
}
