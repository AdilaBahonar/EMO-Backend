namespace EMO.Repositories.ApplianceRedisRepo
{
    public interface IApplianceRedisCacheService
    {
        Task SetApplianceAsync(Guid applianceId);
        Task DeleteApplianceAsync(Guid applianceId);
        Task RebuildAllAppliancesAsync();
        Task RefreshSensorChainsForApplianceAsync(Guid applianceId);
        Task<bool> HasApplianceCacheAsync();
    }
}
