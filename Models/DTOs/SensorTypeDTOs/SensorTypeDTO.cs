// ========================= DTOs =========================
using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SensorTypeDTOs
{
    public class AddSensorTypeDTO
    {
        [Required]
        public string sensorTypeName { get; set; } = string.Empty;

        // optional
        public bool is_active { get; set; } = false;

        // optional
        public int is_type { get; set; } = 0;
    }

    public class UpdateSensorTypeDTO
    {
        [Required]
        public string sensorTypeId { get; set; } = string.Empty;

        public string sensorTypeName { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public int is_type { get; set; } = 0;
    }

    public class SensorTypeResponseDTO
    {
        [Required]
        public string sensorTypeId { get; set; } = string.Empty;

        public string sensorTypeName { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public int is_type { get; set; } = 0;
    }
}
