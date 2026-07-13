namespace EMO.Models.DTOs.SensorChainRedisDTOs
{
    public class SensorChainRedisDTO
    {
        public Guid SensorId { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public string MeterId { get; set; } = string.Empty;
        public string SerialAddress { get; set; } = string.Empty;
        public bool StandbyAutoOff { get; set; }

        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;

        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public bool OfficeIsActive { get; set; }
        public bool OfficeIsOccupied { get; set; }
        public bool OfficeIs24Hours { get; set; }
        public bool OfficeAfterHoursAlertEnabled { get; set; }
        public TimeOnly OfficeOpeningTime { get; set; } = new(9, 0);
        public TimeOnly OfficeClosingTime { get; set; } = new(18, 0);
        public string OfficeWorkingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;

        public Guid FloorId { get; set; }
        public string FloorName { get; set; } = string.Empty;

        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;

        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;

        public Guid? UtilityId { get; set; }
        public string UtilityName { get; set; } = string.Empty;

        // Active appliance assigned to this sensor. Null/empty when no assignment exists.
        public Guid? ApplianceId { get; set; }
        public string ApplianceName { get; set; } = string.Empty;
        public string ApplianceCompanyName { get; set; } = string.Empty;
        public string ApplianceModelNumber { get; set; } = string.Empty;
        public double RatedVoltage { get; set; }
        public double MinCurrent { get; set; }
        public double MaxCurrent { get; set; }
        public double MinPower { get; set; }
        public double MaxPower { get; set; }
        public double StandbyPower { get; set; }
        public double NormalPowerFactor { get; set; }
        public string AppliancePriorityLevel { get; set; } = "Normal";
        public bool ApplianceIsCritical { get; set; }

        // HVAC loop settings, only meaningful for HVAC sensors.
        public Guid? HvacLoopSettingId { get; set; }
        public bool HvacLoopEnabled { get; set; }
        public int HvacLoopOnSeconds { get; set; }
        public int HvacLoopOffSeconds { get; set; }
        public DateTime? HvacLoopStartedAt { get; set; }

        public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
