using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.FloorDTOs
{
    public class AddFloorDTO
    {
        [Required]
        public string floorName { get; set; } = string.Empty;
        [Required]
        public string fkBuilding { get; set; } = string.Empty;
        [Required]
        public string fkBusiness { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
        public bool isActive { get; set; } = false;
    }
    public class UpdateFloorDTO
    {
        [Required]
        public string floorId { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
        public string fkBuilding { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
    public class FloorResponseDTO
    {
        [Required]
        public string floorId { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
        public string fkBuilding { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string buildingName { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
}
