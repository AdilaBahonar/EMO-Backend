using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.HvacLoopSettingDTOs
{
    public class AddHvacLoopSettingDTO
    {
        [Required]
        public string fkSensor { get; set; } = string.Empty;

        public bool loopEnabled { get; set; } = false;

        public int loopOnSeconds { get; set; } = 0;
        public int loopOffSeconds { get; set; } = 0;

        public bool isActive { get; set; } = true;
    }

    public class UpdateHvacLoopSettingDTO
    {
        [Required]
        public string hvacLoopSettingId { get; set; } = string.Empty;

        // Optional. If empty, existing sensor will remain unchanged.
        public string fkSensor { get; set; } = string.Empty;

        public bool loopEnabled { get; set; } = false;

        public int loopOnSeconds { get; set; } = 0;
        public int loopOffSeconds { get; set; } = 0;

        public bool isActive { get; set; } = true;
    }

    public class HvacLoopSensorDTO
    {
        [Required]
        public string fkSensor { get; set; } = string.Empty;
    }

    public class HvacLoopSettingResponseDTO
    {
        public string hvacLoopSettingId { get; set; } = string.Empty;

        public string fkSensor { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;

        public string utilityName { get; set; } = string.Empty;

        public bool loopEnabled { get; set; } = false;

        public int loopOnSeconds { get; set; } = 0;
        public int loopOffSeconds { get; set; } = 0;

        public string loopStartedAt { get; set; } = string.Empty;

        public bool isActive { get; set; } = true;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
    }
}
