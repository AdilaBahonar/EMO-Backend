using EMO.Models.DBModels;
using EMO.Repositories.DeviceRedisRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.RedisStartupService
{
    public class DeviceMacRedisStartupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DeviceMacRedisStartupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            var redisService = scope.ServiceProvider.GetRequiredService<IDeviceRedisService>();

            var exists = await redisService.ValidMacSetExistsAsync();

            if (exists) return;

            var macAddresses = await db.tbl_device
                .Where(x => x.is_active && !x.is_deleted && x.mac_address != null)
                .Select(x => x.mac_address)
                .ToListAsync(cancellationToken);

            await redisService.RebuildValidMacSetAsync(macAddresses);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
