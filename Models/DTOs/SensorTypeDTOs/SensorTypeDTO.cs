// ========================= DTOs =========================
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SensorTypeDTOs
{
    public class AddSensorTypeDTO
    {
        [Required]
        public string sensorTypeName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public int isType { get; set; } = 0;
    }

    public class UpdateSensorTypeDTO
    {
        [Required]
        public string sensorTypeId { get; set; } = string.Empty;

        public string sensorTypeName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public int isType { get; set; } = 0;
    }

    public class SensorTypeResponseDTO
    {
        [Required]
        public string sensorTypeId { get; set; } = string.Empty;
        public string sensorTypeName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public int isType { get; set; } = 0;
    }
}
