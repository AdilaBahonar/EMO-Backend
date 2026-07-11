using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorChainRedisDTOs;
using EMO.Models.DTOs.SensorCommandDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Http.Json;
using System.Text.Json;

namespace EMO.Repositories.SensorCommandRepo
{
    public class SensorCommandService : ISensorCommandService
    {
        private readonly DBUserManagementContext db;
        private readonly IDatabase redis;
        private readonly RedisKeys redisKeys;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SensorCommandService(
            DBUserManagementContext db,
            IConnectionMultiplexer redis,
            IOptions<RedisKeys> redisKeysOptions,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            this.db = db;
            this.redis = redis.GetDatabase();
            redisKeys = redisKeysOptions.Value;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public async Task<ResponseModel<SensorRelayCommandResponseDTO>> SendRelayCommandAsync(SensorRelayCommandRequestDTO requestDto)
        {
            try
            {
                if (!Guid.TryParse(requestDto.SensorId, out var sensorId))
                    return Fail("Invalid sensor id.");

                var command = (requestDto.Command ?? string.Empty).Trim().ToUpperInvariant();
                if (command != "ON" && command != "OFF")
                    return Fail("Command must be ON or OFF.");

                var sensorExists = await db.tbl_sensor.AnyAsync(x => x.sensor_id == sensorId && !x.is_deleted);
                if (!sensorExists)
                    return Fail("Sensor not found.");

                var chain = await GetSensorChainAsync(sensorId);
                if (chain == null)
                    return Fail("Sensor chain is missing in Redis. Refresh sensor chain cache first.");

                if (string.IsNullOrWhiteSpace(chain.MacAddress))
                    return Fail("Device MAC address is missing from sensor chain.");

                if (string.IsNullOrWhiteSpace(chain.SerialAddress))
                    return Fail("Sensor serial address is missing from sensor chain.");

                var nodeHelperBaseUrl = configuration["NodeHelper:BaseUrl"] ?? "http://localhost:3000";
                var mac = CleanMac(chain.MacAddress);
                var client = httpClientFactory.CreateClient();

                var payload = new
                {
                    sensorId = sensorId.ToString(),
                    serialAddress = chain.SerialAddress,
                    address = chain.SerialAddress,
                    relay = command,
                    state = command == "ON",
                    reason = string.IsNullOrWhiteSpace(requestDto.Reason) ? "dashboard_command" : requestDto.Reason,
                    ts = DateTime.UtcNow.ToString("O")
                };

                var url = $"{nodeHelperBaseUrl.TrimEnd('/')}/api/device/{mac}/cmd";
                var httpResponse = await client.PostAsJsonAsync(url, payload);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var body = await httpResponse.Content.ReadAsStringAsync();
                    return Fail($"Node Helper command failed: {(int)httpResponse.StatusCode} {body}");
                }

                return new ResponseModel<SensorRelayCommandResponseDTO>
                {
                    success = true,
                    remarks = "Command sent successfully.",
                    data = new SensorRelayCommandResponseDTO
                    {
                        SensorId = sensorId.ToString(),
                        MacAddress = mac,
                        Topic = $"device/{mac}/cmd",
                        Command = command,
                        Reason = payload.reason,
                        SentAtUtc = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail($"Error: {ex.Message}");
            }
        }

        private async Task<SensorChainRedisDTO?> GetSensorChainAsync(Guid sensorId)
        {
            var json = await redis.StringGetAsync($"{redisKeys.SensorChainKeyPrefix}{sensorId}");
            if (!json.HasValue) return null;
            return JsonSerializer.Deserialize<SensorChainRedisDTO>(json!, jsonOptions);
        }

        private static string CleanMac(string mac)
        {
            return (mac ?? string.Empty).Replace(":", string.Empty).Replace("-", string.Empty).Trim().ToLowerInvariant();
        }

        private static ResponseModel<SensorRelayCommandResponseDTO> Fail(string message)
        {
            return new ResponseModel<SensorRelayCommandResponseDTO>
            {
                success = false,
                remarks = message
            };
        }
    }
}
