namespace EMO.Repositories.DeviceRedisRepo
{
    public interface IDeviceRedisService
    {
        Task AddMacAsync(string? macAddress);
        Task RemoveMacAsync(string? macAddress);
        Task UpdateMacAsync(string? oldMacAddress, string? newMacAddress, bool isActive);
        Task<bool> ValidMacSetExistsAsync();
        Task RebuildValidMacSetAsync(List<string> macAddresses);
    }
}
