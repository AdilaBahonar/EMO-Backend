using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.BuildingDTOs
{
    public class AddBuildingDTO
    {
        [Required]
        public string buildingName { get; set; } = string.Empty;
        [Required]
        public string fkFacility { get; set; } = string.Empty;
    }
    public class UpdateBuildingDTO
    {
        [Required]
        public string buildingId { get; set; } = string.Empty;
        public string buildingName { get; set; } = string.Empty;
        public string fkFacility { get; set; } = string.Empty;
    }
    public class BuildingResponseDTO
    {
        [Required]
        public string buildingId { get; set; } = string.Empty;
        public string buildingName { get; set; } = string.Empty;
        public string facilityName { get; set; } = string.Empty;
        public string fkFacility { get; set; } = string.Empty;
    }
}
