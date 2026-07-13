using EMO.Repositories.DemandManagementRedisRepo;

namespace EMO.Repositories.RedisStartupService
{
    /// <summary>
    /// Rebuilds demand:config:* when the API starts so the live worker never
    /// needs to call PostgreSQL for threshold decisions.
    /// </summary>
    public class DemandManagementRedisStartupCacheService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DemandManagementRedisStartupCacheService> logger;

        public DemandManagementRedisStartupCacheService(
            IServiceProvider serviceProvider,
            ILogger<DemandManagementRedisStartupCacheService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IDemandManagementRedisCacheService>();
                await service.RebuildAllAsync();
                logger.LogInformation("Demand-management Redis cache rebuilt on startup.");
            }
            catch (Exception exception)
            {
                // Do not take the API down if Redis is temporarily unavailable.
                // The next demand-setting save will refresh that business cache.
                logger.LogError(exception, "Demand-management Redis startup cache could not be rebuilt.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
