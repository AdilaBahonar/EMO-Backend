using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.DeviceDTOs
{
    public class AddDeviceDTO
    {
        [Required]
        public string deviceName { get; set; } = string.Empty;  
    }
    public class UpdateDeviceDTO
    {
        [Required]
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
    public class DeviceResponseDTO
    {
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
    }
}
