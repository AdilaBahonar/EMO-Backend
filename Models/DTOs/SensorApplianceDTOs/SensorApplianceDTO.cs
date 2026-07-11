using System.ComponentModel.DataAnnotations;
using EMO.Models.DTOs.ApplianceDTOs;

namespace EMO.Models.DTOs.SensorApplianceDTOs
{
    public class AssignSensorApplianceDTO
    {
        [Required]
        public string fkSensor { get; set; } = string.Empty;

        [Required]
        public string fkAppliance { get; set; } = string.Empty;

        public string remarks { get; set; } = string.Empty;
        public bool isActive { get; set; } = true;
    }

    public class UpdateSensorApplianceDTO
    {
        [Required]
        public string sensorApplianceId { get; set; } = string.Empty;

        public string fkSensor { get; set; } = string.Empty;
        public string fkAppliance { get; set; } = string.Empty;

        public string remarks { get; set; } = string.Empty;
        public bool isActive { get; set; } = true;
    }

    public class SensorAssignableAppliancesDTO
    {
        public string fkSensor { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string fkUtility { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;
        public List<ApplianceResponseDTO> appliances { get; set; } = new();
    }

    public class SensorApplianceResponseDTO
    {
        public string sensorApplianceId { get; set; } = string.Empty;

        public string fkSensor { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;

        public string fkAppliance { get; set; } = string.Empty;
        public string applianceName { get; set; } = string.Empty;
        public string applianceType { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public string modelNumber { get; set; } = string.Empty;

        public string fkBusiness { get; set; } = string.Empty;
        public string fkUtility { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;

        public float ratedVoltage { get; set; } = 220;
        public float minCurrent { get; set; } = 0;
        public float maxCurrent { get; set; } = 0;
        public float minPower { get; set; } = 0;
        public float maxPower { get; set; } = 0;
        public float standbyPower { get; set; } = 0;
        public float normalPowerFactor { get; set; } = 0;

        public bool isShiftable { get; set; } = false;
        public bool isCritical { get; set; } = false;
        public string priorityLevel { get; set; } = "Normal";
        public string normalOperatingHours { get; set; } = string.Empty;
        public bool canAutoControl { get; set; } = false;
        public string allowedShiftStartTime { get; set; } = string.Empty;
        public string allowedShiftEndTime { get; set; } = string.Empty;
        public bool allowOptimizationSuggestions { get; set; } = true;
        public int minimumOnDurationMinutes { get; set; } = 0;
        public int minimumOffDurationMinutes { get; set; } = 0;

        public string remarks { get; set; } = string.Empty;
        public string assignedAt { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;

        public bool isActive { get; set; } = true;
    }

    public class ApplianceStatusDTO
    {
        public string sensorId { get; set; } = string.Empty;
        public string sensorName { get; set; } = string.Empty;

        public string applianceId { get; set; } = string.Empty;
        public string applianceName { get; set; } = string.Empty;
        public string applianceType { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public string modelNumber { get; set; } = string.Empty;
        public string utilityName { get; set; } = string.Empty;

        public float actualCurrent { get; set; } = 0;
        public float actualPower { get; set; } = 0;
        public float actualPowerFactor { get; set; } = 0;

        public float minCurrent { get; set; } = 0;
        public float maxCurrent { get; set; } = 0;
        public float minPower { get; set; } = 0;
        public float maxPower { get; set; } = 0;
        public float standbyPower { get; set; } = 0;
        public float normalPowerFactor { get; set; } = 0;

        public string status { get; set; } = string.Empty;
        public string alertMessage { get; set; } = string.Empty;
        public string lastReadingAt { get; set; } = string.Empty;
    }
}
