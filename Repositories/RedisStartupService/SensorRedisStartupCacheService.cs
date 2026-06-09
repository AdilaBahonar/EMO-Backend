using EMO.Repositories.SensorsChainRepo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EMO.Repositories.RedisStartupService
{


    public class SensorRedisStartupCacheService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public SensorRedisStartupCacheService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var sensorRedisCacheService =
                scope.ServiceProvider.GetRequiredService<ISensorRedisCacheService>();

            var hasCache = await sensorRedisCacheService.HasSensorCacheAsync();

            if (!hasCache)
            {
                await sensorRedisCacheService.LoadAllSensorsChainAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
