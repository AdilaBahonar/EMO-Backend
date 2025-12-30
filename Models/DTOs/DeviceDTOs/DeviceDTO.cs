using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.DeviceDTOs
{
    public class AddDeviceDTO
    {
        [Required]
        public string deviceName { get; set; } = string.Empty;
        [Required]
        public string fkOffice { get; set; } = string.Empty;
        [Required]
        public string fkDeviceType { get; set; } = string.Empty;
        [Required]
        public string fkControlType { get; set; } = string.Empty;
    }
    public class UpdateDeviceDTO
    {
        [Required]
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string fkOffice { get; set; } = string.Empty;
        public string fkDeviceType { get; set; } = string.Empty;
        public string fkControlType { get; set; } = string.Empty;
    }
    public class DeviceResponseDTO
    {
        public string deviceId { get; set; } = string.Empty;
        public string deviceName { get; set; } = string.Empty;
        public string fkOffice { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string fkDeviceType { get; set; } = string.Empty;
        public string deviceTypeName { get; set; } = string.Empty;
        public string fkControlType { get; set; } = string.Empty;
        public string controlTypeName { get; set; } = string.Empty;
    }
}
