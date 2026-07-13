namespace EMO.Repositories.DemandManagementRedisRepo
{
    public interface IDemandManagementRedisCacheService
    {
        Task SetBusinessAsync(Guid businessId, string reason = "demand_configuration_saved");
        Task DeleteBusinessAsync(Guid businessId, string reason = "demand_configuration_deleted");
        Task RebuildAllAsync();
        Task PublishChangedAsync(Guid businessId, string reason);
    }
}
