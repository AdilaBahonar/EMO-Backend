using EMO.Repositories.SensorsChainRepo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMO.Repositories.RedisStartupService
{
    public class SensorRedisStartupCacheService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<SensorRedisStartupCacheService> logger;

        public SensorRedisStartupCacheService(
            IServiceProvider serviceProvider,
            ILogger<SensorRedisStartupCacheService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var sensorRedisCacheService =
                scope.ServiceProvider.GetRequiredService<ISensorRedisCacheService>();

            // Do not only check "does any key exist".
            // Redis can be partially stale after restart, so rebuild from PostgreSQL source of truth.
            await sensorRedisCacheService.RebuildAllSensorsChainAsync();

            logger.LogInformation("Sensor Redis chain cache rebuilt on startup.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
