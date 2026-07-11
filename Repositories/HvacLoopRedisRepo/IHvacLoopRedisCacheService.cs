namespace EMO.Repositories.HvacLoopRedisRepo
{
    public interface IHvacLoopRedisCacheService
    {
        Task SetLoopSettingAsync(Guid sensorId, string reason = "loop_setting_saved");
        Task DeleteLoopSettingAsync(Guid sensorId);
        Task RebuildAllLoopSettingsAsync();
        Task PublishLoopChangedAsync(Guid sensorId, string reason);
    }
}
