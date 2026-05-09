using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.DeviceDTOs
{
    public class AddDeviceDTO
    {
        [Required]
        public string deviceName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
    public class UpdateDeviceDTO
    {
        [Required]
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }
    public class DeviceResponseDTO
    {
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }

    public class DeviceSensorsResponseDTO
    {
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public List<sensorOfDeviceResponseDTO> sensors { get; set; } = new List<sensorOfDeviceResponseDTO>();
    }

    public class sensorOfDeviceResponseDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string modeBusAddress { get; set; } = string.Empty;
        public string serialAddress { get; set; } = string.Empty;
    }
}
