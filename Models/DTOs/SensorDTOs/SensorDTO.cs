using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.SensorDTOs
{
    public class AddSensorDTO
    {
        [Required]
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        [Required]
        public string fkSensortype { get; set; } = string.Empty;
        [Required]
        public string fkOffice { get; set; } = string.Empty;
        [Required]
        public string fkDevice { get; set; } = string.Empty;
        [Required]
        public string fkutility { get; set; } = string.Empty;

    }
    
    public class UpdateSensorDTO
    {
        [Required]
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        public string fkSensortype { get; set; } = string.Empty;
        public string fkOffice { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;
    }
    
    public class SensorResponseDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        public string fkSensortype { get; set; } = string.Empty;
        public string sensorTypeName { get; set; } = string.Empty;
        public string fkOffice { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;
    }
}

