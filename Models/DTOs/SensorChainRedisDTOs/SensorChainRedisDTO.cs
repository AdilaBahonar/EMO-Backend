namespace EMO.Models.DTOs.SensorChainRedisDTOs
{
    public class SensorChainRedisDTO
    {
        public Guid SensorId { get; set; }
        public string SensorName { get; set; }

        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; }

        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; }

        public Guid SectionId { get; set; }
        public string SectionName { get; set; }

        public Guid FloorId { get; set; }
        public string FloorName { get; set; }

        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }

        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }

        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; }

        public Guid? UtilityId { get; set; }
        public string UtilityName { get; set; }
    }
}
