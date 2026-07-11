using EMO.Repositories.ApplianceRedisRepo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMO.Repositories.RedisStartupService
{
    public class ApplianceRedisStartupCacheService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ApplianceRedisStartupCacheService> logger;

        public ApplianceRedisStartupCacheService(
            IServiceProvider serviceProvider,
            ILogger<ApplianceRedisStartupCacheService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IApplianceRedisCacheService>();

            // Always rebuild appliance cache from DB so standby/max power thresholds stay correct.
            await service.RebuildAllAppliancesAsync();

            logger.LogInformation("Appliance Redis cache rebuilt on startup.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
