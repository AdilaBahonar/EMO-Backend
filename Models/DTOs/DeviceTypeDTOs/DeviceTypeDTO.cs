using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.DeviceTypeDTOs
{
    public class AddDeviceTypeDTO
    {
        [Required]
        public string deviceTypeName { get; set; } = string.Empty;

    }

    public class UpdateDeviceTypeDTO
    {
        [Required]
        public string deviceTypeId { get; set; } = string.Empty;
        public string deviceTypeName { get; set; } = string.Empty;

    }

    public class DeviceTypeResponseDTO
    {
        public string deviceTypeId { get; set; } = string.Empty;
        public string deviceTypeName { get; set; } = string.Empty;
    }
}
