using StackExchange.Redis;

namespace EMO.Repositories.DeviceRedisRepo
{
    public class DeviceRedisService : IDeviceRedisService
    {
        private readonly IDatabase _redis;
        private readonly string _key;

        public DeviceRedisService(IConnectionMultiplexer redis, IConfiguration config)
        {
            _redis = redis.GetDatabase();
            _key = config["Redis:ValidDeviceMacSetKey"] ?? "valid:devices:macs";
        }

        private static string NormalizeMac(string? mac)
        {
            return string.IsNullOrWhiteSpace(mac)
                ? ""
                : mac.Replace(":", "").Replace("-", "").Trim().ToLower();
        }

        public async Task AddMacAsync(string? macAddress)
        {
            var mac = NormalizeMac(macAddress);
            if (string.IsNullOrEmpty(mac)) return;

            await _redis.SetAddAsync(_key, mac);
        }

        public async Task RemoveMacAsync(string? macAddress)
        {
            var mac = NormalizeMac(macAddress);
            if (string.IsNullOrEmpty(mac)) return;

            await _redis.SetRemoveAsync(_key, mac);
        }

        public async Task UpdateMacAsync(string? oldMacAddress, string? newMacAddress, bool isActive)
        {
            var oldMac = NormalizeMac(oldMacAddress);
            var newMac = NormalizeMac(newMacAddress);

            if (!string.IsNullOrEmpty(oldMac) && oldMac != newMac)
                await _redis.SetRemoveAsync(_key, oldMac);

            if (isActive && !string.IsNullOrEmpty(newMac))
                await _redis.SetAddAsync(_key, newMac);
            else if (!string.IsNullOrEmpty(newMac))
                await _redis.SetRemoveAsync(_key, newMac);
        }

        public async Task<bool> ValidMacSetExistsAsync()
        {
            return await _redis.KeyExistsAsync(_key);
        }

        public async Task RebuildValidMacSetAsync(List<string> macAddresses)
        {
            await _redis.KeyDeleteAsync(_key);

            var values = macAddresses
                .Select(NormalizeMac)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .Select(x => (RedisValue)x)
                .ToArray();

            if (values.Length > 0)
                await _redis.SetAddAsync(_key, values);
        }
    }
}
