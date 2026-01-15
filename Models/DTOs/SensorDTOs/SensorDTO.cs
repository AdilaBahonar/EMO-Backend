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
        public string fkSensortype { get; set; } = string.Empty;
        public string fkMeterTypeDetail { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;

    }
    public class UpdateInnerSensorDTO
    {
        [Required]
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        public string fkSensortype { get; set; } = string.Empty;
        public string fkMeterTypeDetail { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
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
        public string fkMeterTypeDetail { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;
    }
    public class sensorResponseDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        public string fkSensortype { get; set; } = string.Empty;
        public string fkMeterTypeDetail { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;
    }
    public class SensorInnerResponseDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modebusAddress { get; set; } = string.Empty;
        public string meterId { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
        public string fkSensortype { get; set; } = string.Empty;
        public string fkMeterTypeDetail { get; set; } = string.Empty;
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
        public string fkMeterTypeDetail { get; set; } = string.Empty;
        public string fkDevice { get; set; } = string.Empty;
        public string fkutility { get; set; } = string.Empty;
    }
}

}
