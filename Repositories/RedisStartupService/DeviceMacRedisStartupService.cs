using EMO.Models.DBModels;
using EMO.Repositories.DeviceRedisRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMO.Repositories.RedisStartupService
{
    public class DeviceMacRedisStartupService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DeviceMacRedisStartupService> logger;

        public DeviceMacRedisStartupService(
            IServiceProvider serviceProvider,
            ILogger<DeviceMacRedisStartupService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            var redisService = scope.ServiceProvider.GetRequiredService<IDeviceRedisService>();

            // Always rebuild this set from DB. If Redis persisted old MACs, simulator validation can be wrong.
            var macAddresses = await db.tbl_device
                .Where(x => x.is_active && !x.is_deleted && x.mac_address != null)
                .Select(x => x.mac_address)
                .ToListAsync(cancellationToken);

            await redisService.RebuildValidMacSetAsync(macAddresses);

            logger.LogInformation("Valid device MAC Redis set rebuilt on startup. Count: {Count}", macAddresses.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
