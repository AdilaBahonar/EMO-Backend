using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.ApplianceDTOs
{
    public class AddApplianceDTO
    {
        [Required]
        public string applianceName { get; set; } = string.Empty;

        public string companyName { get; set; } = string.Empty;
        public string modelNumber { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;

        public float ratedVoltage { get; set; } = 220;

        public float minCurrent { get; set; } = 0;
        public float maxCurrent { get; set; } = 0;

        public float minPower { get; set; } = 0;
        public float maxPower { get; set; } = 0;

        public float standbyPower { get; set; } = 0;
        public float normalPowerFactor { get; set; } = 0;

        public string description { get; set; } = string.Empty;

        public bool isShiftable { get; set; } = false;
        public string priorityLevel { get; set; } = "Normal";
        public string normalOperatingHours { get; set; } = string.Empty;
        public bool canAutoControl { get; set; } = false;
        public bool isCritical { get; set; } = false;
        public bool allowOptimizationSuggestions { get; set; } = true;
        public string allowedShiftStartTime { get; set; } = string.Empty;
        public string allowedShiftEndTime { get; set; } = string.Empty;
        public int minimumOnDurationMinutes { get; set; } = 0;
        public int minimumOffDurationMinutes { get; set; } = 0;

        public bool isDefault { get; set; } = false;
        public bool isCustom { get; set; } = true;
        public bool isActive { get; set; } = true;

        [Required]
        public string fkUtility { get; set; } = string.Empty;
    }

    public class UpdateApplianceDTO
    {
        [Required]
        public string applianceId { get; set; } = string.Empty;

        public string applianceName { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public string modelNumber { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;

        public float ratedVoltage { get; set; } = 220;

        public float minCurrent { get; set; } = 0;
        public float maxCurrent { get; set; } = 0;

        public float minPower { get; set; } = 0;
        public float maxPower { get; set; } = 0;

        public float standbyPower { get; set; } = 0;
        public float normalPowerFactor { get; set; } = 0;

        public string description { get; set; } = string.Empty;

        public bool isShiftable { get; set; } = false;
        public string priorityLevel { get; set; } = "Normal";
        public string normalOperatingHours { get; set; } = string.Empty;
        public bool canAutoControl { get; set; } = false;
        public bool isCritical { get; set; } = false;
        public bool allowOptimizationSuggestions { get; set; } = true;
        public string allowedShiftStartTime { get; set; } = string.Empty;
        public string allowedShiftEndTime { get; set; } = string.Empty;
        public int minimumOnDurationMinutes { get; set; } = 0;
        public int minimumOffDurationMinutes { get; set; } = 0;

        public bool isDefault { get; set; } = false;
        public bool isCustom { get; set; } = true;
        public bool isActive { get; set; } = true;

        public string fkUtility { get; set; } = string.Empty;
    }

    public class ApplianceResponseDTO
    {
        public string applianceId { get; set; } = string.Empty;
        public string applianceName { get; set; } = string.Empty;

        public string companyName { get; set; } = string.Empty;
        public string modelNumber { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string fkDefaultAppliance { get; set; } = string.Empty;

        public float ratedVoltage { get; set; } = 220;

        public float minCurrent { get; set; } = 0;
        public float maxCurrent { get; set; } = 0;

        public float minPower { get; set; } = 0;
        public float maxPower { get; set; } = 0;

        public float standbyPower { get; set; } = 0;
        public float normalPowerFactor { get; set; } = 0;

        public string description { get; set; } = string.Empty;

        public bool isShiftable { get; set; } = false;
        public string priorityLevel { get; set; } = "Normal";
        public string normalOperatingHours { get; set; } = string.Empty;
        public bool canAutoControl { get; set; } = false;
        public bool isCritical { get; set; } = false;
        public bool allowOptimizationSuggestions { get; set; } = true;
        public string allowedShiftStartTime { get; set; } = string.Empty;
        public string allowedShiftEndTime { get; set; } = string.Empty;
        public int minimumOnDurationMinutes { get; set; } = 0;
        public int minimumOffDurationMinutes { get; set; } = 0;

        public bool isDefault { get; set; } = false;
        public bool isCustom { get; set; } = true;

        public string fkUtility { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;

        public bool isActive { get; set; } = true;
    }
}
