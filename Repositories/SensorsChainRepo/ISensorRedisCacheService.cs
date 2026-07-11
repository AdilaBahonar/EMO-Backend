namespace EMO.Repositories.SensorsChainRepo
{
    public interface ISensorRedisCacheService
    {
        Task SetSensorChainAsync(Guid sensorId);
        Task DeleteSensorChainAsync(Guid sensorId);
        Task<bool> HasSensorCacheAsync();
        Task<bool> HasSensorChainAsync(Guid sensorId);
        Task LoadAllSensorsChainAsync();
        Task EnsureAllSensorsChainAsync();
        Task RebuildAllSensorsChainAsync();
    }
}
