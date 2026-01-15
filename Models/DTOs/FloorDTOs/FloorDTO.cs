using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.FloorDTOs
{
    public class AddFloorDTO
    {
        [Required]
        public string floorName { get; set; } = string.Empty;
        [Required]
        public string fkBuilding { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
    }
    public class UpdateFloorDTO
    {
        [Required]
        public string floorId { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
        public string fkBuilding { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
    public class FloorResponseDTO
    {
        [Required]
        public string floorId { get; set; } = string.Empty;
        public string floorName { get; set; } = string.Empty;
        public int floorNo { get; set; } = 0;
        public string fkBuilding { get; set; } = string.Empty;
        public string buildingName { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
