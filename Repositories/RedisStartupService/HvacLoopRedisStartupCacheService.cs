using EMO.Repositories.HvacLoopRedisRepo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMO.Repositories.RedisStartupService
{
    public class HvacLoopRedisStartupCacheService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<HvacLoopRedisStartupCacheService> logger;

        public HvacLoopRedisStartupCacheService(
            IServiceProvider serviceProvider,
            ILogger<HvacLoopRedisStartupCacheService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IHvacLoopRedisCacheService>();

            // Always rebuild loop keys and hvac:loop:enabled set from DB.
            await service.RebuildAllLoopSettingsAsync();

            logger.LogInformation("HVAC loop Redis cache rebuilt on startup.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
