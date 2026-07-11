namespace EMO.Models.DTOs.SensorCommandDTOs
{
    public class SensorRelayCommandRequestDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string Command { get; set; } = "OFF"; // ON|OFF
        public string Reason { get; set; } = "manual_dashboard_command";
    }

    public class SensorRelayCommandResponseDTO
    {
        public string SensorId { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    }
}
