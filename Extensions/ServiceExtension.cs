/*//using EMO.Extensions.MiddleWare;
//using EMO.Models.DBModels;
//using EMO.Repositories.AgreementServicesRepo;
//using EMO.Repositories.ApiKeyServiceRepo;
//using EMO.Repositories.ApplianceRedisRepo;
//using EMO.Repositories.ApplianceServicesRepo;
//using EMO.Repositories.AuthServicesRepo;
//using EMO.Repositories.BuildingServicesRepo;
//using EMO.Repositories.BusinessServicesRepo;
//using EMO.Repositories.ContactPersonServicesRepo;
//using EMO.Repositories.DashboardServicesRepo;
//using EMO.Repositories.DeviceRedisRepo;
//using EMO.Repositories.DeviceServicesRepo;
//using EMO.Repositories.EnergyDashboardRepo;
//using EMO.Repositories.EnergyDashboardServicesRepo;
//using EMO.Repositories.FacilityServicesRepo;
//using EMO.Repositories.FloorServicesRepo;
//using EMO.Repositories.GenderServicesRepo;
//using EMO.Repositories.HvacLoopRedisRepo;
//using EMO.Repositories.HvacLoopSettingServicesRepo;
//using EMO.Repositories.InnerServicesRepo;
//using EMO.Repositories.JWTUtilsRepo;
//using EMO.Repositories.OfficeServicesRepo;
//using EMO.Repositories.OptimizationDashboardRepo;
//using EMO.Repositories.RedisStartupService;
//using EMO.Repositories.SectionServicesRepo;
//using EMO.Repositories.SensorApplianceServicesRepo;
//using EMO.Repositories.SensorCommandRepo;
//using EMO.Repositories.SensorsChainRepo;
//using EMO.Repositories.SensorServicesRepo;
//using EMO.Repositories.SingalPhaseDataRepo;
//using EMO.Repositories.SingalPhaseDataServicesRepo;
//using EMO.Repositories.SubUserTypeServicesRepo;
//using EMO.Repositories.SuperAdminDashboardServicesRepo;
//using EMO.Repositories.TenantServicesRepo;
//using EMO.Repositories.UserServicesRepo;
//using EMO.Repositories.UserTypeServicesRepo;
//using EMO.Repositories.UtilityServicesRepo;

//using EnergyMonitor.Services;
//using Microsoft.EntityFrameworkCore;
//using StackExchange.Redis;

//namespace EMO.Extensions
//{
//    public static class ServiceExtension
//    {
//        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
//        {
//            services.AddDbContext<DBUserManagementContext>(options =>
//            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

//            services.Configure<RedisKeys>(
//            configuration.GetSection("RedisKeys"));

//            services.AddSingleton<IConnectionMultiplexer>(sp =>
//            {
//                var redisConnection =
//                    configuration.GetConnectionString("Redis");

//                return ConnectionMultiplexer.Connect(redisConnection);
//            });

//            //services.AddDbContext<DBUserManagementContext>(options =>
//            //options.UseMySql(
//            //    configuration.GetConnectionString("DefaultConnection"),
//            //    new MySqlServerVersion(new Version(8, 0, 32)) // replace with your MySQL version
//            //));
//            // Adding Cors
//            services.AddCors(options =>
//            {
//                options.AddPolicy(name: "AllowedOrigins",
//                                  policy =>
//                                  {
//                                      policy
//                                      .AllowAnyHeader()
//                                      .AllowAnyOrigin()
//                                      .AllowAnyMethod();
//                                  });
//            });
//            services.AddHostedService<SensorRedisStartupCacheService>();
//            services.AddHostedService<DeviceMacRedisStartupService>();
//            services.AddHostedService<ApplianceRedisStartupCacheService>();
//            services.AddHostedService<HvacLoopRedisStartupCacheService>();
//            services.AddHostedService<DashboardAggregateWorker>();
//            services.AddAutoMapper(typeof(Program).Assembly);
//            services.AddTransient<IJWTUtils, JWTUtils>();
//            services.AddTransient<IApiKeyService, ApiKeyService>();
//            services.AddScoped<ISensorRedisCacheService, SensorRedisCacheService>();
//            services.AddScoped<IDeviceRedisService, DeviceRedisService>();
//            services.AddSingleton<UserAuthorizeAttribute>();
//            services.AddSingleton<ApiKeyAttribute>();
//            services.AddTransient<OtherServices>();
//            services.AddHttpClient();
//            services.AddScoped<ISensorCommandService, SensorCommandService>();
//            services.AddTransient<IUserServices, UserServices>();
//            services.AddTransient<IAuthServices, AuthServices>();
//            services.AddTransient<IInnerServices, InnerServices>();
//            services.AddTransient<IUserTypeService, UserTypeService>();
//            services.AddTransient<IBusinessServices, BusinessServices>();
//            services.AddTransient<IBuildingServices, BuildingServices>();
//            services.AddTransient<DashboardService>();
//            services.AddScoped<IEnergyDashboardService, EnergyDashboardService>();
//            services.AddTransient<IFacilityServices, FacilityServices>();
//            services.AddTransient<ISectionServices, SectionServices>();
//            services.AddTransient<IFloorServices, FloorServices>();
//            services.AddTransient<IOfficeServices, OfficeServices>();
//            //services.AddTransient<IControlTypeServices, ControlTypeServices>();
//            services.AddTransient<IDeviceServices, DeviceServices>();
//            //services.AddTransient<IDeviceTypeServices, DeviceTypeServices>();
//            services.AddTransient<IContactPersonServices, ContactPersonServices>();
//            services.AddTransient<ISingalPhaseDataService, SingalPhaseDataService>();
//            services.AddTransient<ITenantServices, TenantServices>();
//            services.AddTransient<IUtilityServices, UtilityServices>();
//            services.AddTransient<ISuperAdminDashboardServices, SuperAdminDashboardServices>();
//            services.AddTransient<IAgreementServices, AgreementServices>();
//            services.AddTransient<IGenderServices, GenderServices>();
//            services.AddTransient<ISubUserTypeServices, SubUserTypeServices>();
//            services.AddTransient<ISensorServices, SensorServices>();
//            services.AddTransient<IApplianceServices, ApplianceServices>();
//            services.AddTransient<ISensorApplianceServices, SensorApplianceServices>();
//            services.AddTransient<IHvacLoopSettingServices, HvacLoopSettingServices>();
//            services.AddScoped<IApplianceRedisCacheService, ApplianceRedisCacheService>();
//            services.AddScoped<IHvacLoopRedisCacheService, HvacLoopRedisCacheService>();
//            services.AddScoped<IOptimizationDashboardService, OptimizationDashboardService>();
//        }
//    }
//}
using EMO.Extensions.MiddleWare;
using EMO.Models.DBModels;
using EMO.Repositories.AgreementServicesRepo;
using EMO.Repositories.ApiKeyServiceRepo;
using EMO.Repositories.ApplianceRedisRepo;
using EMO.Repositories.ApplianceServicesRepo;
using EMO.Repositories.AuthServicesRepo;
using EMO.Repositories.BuildingServicesRepo;
using EMO.Repositories.BusinessServicesRepo;
using EMO.Repositories.ContactPersonServicesRepo;
using EMO.Repositories.DashboardServicesRepo;
using EMO.Repositories.DeepDiveRepo;
using EMO.Repositories.DeviceRedisRepo;
using EMO.Repositories.DeviceServicesRepo;
using EMO.Repositories.EnergyConfigurationRepo;
using EMO.Repositories.EnergyDashboardRepo;
using EMO.Repositories.EnergyDashboardServicesRepo;
using EMO.Repositories.FacilityServicesRepo;
using EMO.Repositories.FloorServicesRepo;
using EMO.Repositories.GenderServicesRepo;
using EMO.Repositories.HvacLoopRedisRepo;
using EMO.Repositories.HvacLoopSettingServicesRepo;
using EMO.Repositories.InnerServicesRepo;
using EMO.Repositories.JWTUtilsRepo;
using EMO.Repositories.OfficeServicesRepo;
using EMO.Repositories.OptimizationDashboardRepo;
using EMO.Repositories.OptimizationRuntimeRepo;
using EMO.Repositories.RedisStartupService;
using EMO.Repositories.SectionServicesRepo;
using EMO.Repositories.SensorApplianceServicesRepo;
using EMO.Repositories.SensorCommandRepo;
using EMO.Repositories.SensorsChainRepo;
using EMO.Repositories.SensorServicesRepo;
using EMO.Repositories.SingalPhaseDataRepo;
using EMO.Repositories.SingalPhaseDataServicesRepo;
using EMO.Repositories.SubUserTypeServicesRepo;
using EMO.Repositories.SuperAdminDashboardServicesRepo;
using EMO.Repositories.TenantServicesRepo;
using EMO.Repositories.UserServicesRepo;
using EMO.Repositories.UserTypeServicesRepo;
using EMO.Repositories.UtilityServicesRepo;

using EnergyMonitor.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

using EMO.Repositories.HistoricalDataRepo;
namespace EMO.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DBUserManagementContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<RedisKeys>(
            configuration.GetSection("RedisKeys"));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection =
                    configuration.GetConnectionString("Redis");

                return ConnectionMultiplexer.Connect(redisConnection);
            });

            //services.AddDbContext<DBUserManagementContext>(options =>
            //options.UseMySql(
            //    configuration.GetConnectionString("DefaultConnection"),
            //    new MySqlServerVersion(new Version(8, 0, 32)) // replace with your MySQL version
            //));
            // Adding Cors
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowedOrigins",
                                  policy =>
                                  {
                                      policy
                                      .AllowAnyHeader()
                                      .AllowAnyOrigin()
                                      .AllowAnyMethod();
                                  });
            });
            services.AddHostedService<SensorRedisStartupCacheService>();
            services.AddHostedService<DeviceMacRedisStartupService>();
            services.AddHostedService<ApplianceRedisStartupCacheService>();
            services.AddHostedService<HvacLoopRedisStartupCacheService>();
            services.AddHostedService<DashboardAggregateWorker>();
            services.AddAutoMapper(typeof(Program).Assembly);
            services.AddTransient<IJWTUtils, JWTUtils>();
            services.AddTransient<IApiKeyService, ApiKeyService>();
            services.AddScoped<ISensorRedisCacheService, SensorRedisCacheService>();
            services.AddScoped<IDeviceRedisService, DeviceRedisService>();
            services.AddSingleton<UserAuthorizeAttribute>();
            services.AddSingleton<ApiKeyAttribute>();
            services.AddTransient<OtherServices>();
            services.AddHttpClient();
            services.AddScoped<ISensorCommandService, SensorCommandService>();
            services.AddTransient<IUserServices, UserServices>();
            services.AddTransient<IAuthServices, AuthServices>();
            services.AddTransient<IInnerServices, InnerServices>();
            services.AddTransient<IUserTypeService, UserTypeService>();
            services.AddTransient<IBusinessServices, BusinessServices>();
            services.AddTransient<IBuildingServices, BuildingServices>();
            services.AddTransient<DashboardService>();
            services.AddScoped<IEnergyDashboardService, EnergyDashboardService>();
            services.AddTransient<IFacilityServices, FacilityServices>();
            services.AddTransient<ISectionServices, SectionServices>();
            services.AddTransient<IFloorServices, FloorServices>();
            services.AddTransient<IOfficeServices, OfficeServices>();
            //services.AddTransient<IControlTypeServices, ControlTypeServices>();
            services.AddTransient<IDeviceServices, DeviceServices>();
            //services.AddTransient<IDeviceTypeServices, DeviceTypeServices>();
            services.AddTransient<IContactPersonServices, ContactPersonServices>();
            services.AddTransient<ISingalPhaseDataService, SingalPhaseDataService>();
            services.AddTransient<ITenantServices, TenantServices>();
            services.AddTransient<IUtilityServices, UtilityServices>();
            services.AddTransient<ISuperAdminDashboardServices, SuperAdminDashboardServices>();
            services.AddTransient<IAgreementServices, AgreementServices>();
            services.AddTransient<IGenderServices, GenderServices>();
            services.AddTransient<ISubUserTypeServices, SubUserTypeServices>();
            services.AddTransient<ISensorServices, SensorServices>();
            services.AddTransient<IApplianceServices, ApplianceServices>();
            services.AddTransient<ISensorApplianceServices, SensorApplianceServices>();
            services.AddTransient<IHvacLoopSettingServices, HvacLoopSettingServices>();
            services.AddScoped<IApplianceRedisCacheService, ApplianceRedisCacheService>();
            services.AddScoped<IHvacLoopRedisCacheService, HvacLoopRedisCacheService>();
            services.AddScoped<IOptimizationDashboardService, OptimizationDashboardService>();
            services.AddScoped<IEnergyConfigurationService, EnergyConfigurationService>();
            services.AddScoped<IDeepDiveService, DeepDiveService>();
        }
    }
}
*/


//using EMO.Extensions.MiddleWare;
//using EMO.Models.DBModels;
//using EMO.Repositories.AgreementServicesRepo;
//using EMO.Repositories.ApiKeyServiceRepo;
//using EMO.Repositories.ApplianceRedisRepo;
//using EMO.Repositories.ApplianceServicesRepo;
//using EMO.Repositories.AuthServicesRepo;
//using EMO.Repositories.BuildingServicesRepo;
//using EMO.Repositories.BusinessServicesRepo;
//using EMO.Repositories.ContactPersonServicesRepo;
//using EMO.Repositories.DashboardServicesRepo;
//using EMO.Repositories.DeviceRedisRepo;
//using EMO.Repositories.DeviceServicesRepo;
//using EMO.Repositories.EnergyDashboardRepo;
//using EMO.Repositories.EnergyDashboardServicesRepo;
//using EMO.Repositories.FacilityServicesRepo;
//using EMO.Repositories.FloorServicesRepo;
//using EMO.Repositories.GenderServicesRepo;
//using EMO.Repositories.HvacLoopRedisRepo;
//using EMO.Repositories.HvacLoopSettingServicesRepo;
//using EMO.Repositories.InnerServicesRepo;
//using EMO.Repositories.JWTUtilsRepo;
//using EMO.Repositories.OfficeServicesRepo;
//using EMO.Repositories.OptimizationDashboardRepo;
//using EMO.Repositories.RedisStartupService;
//using EMO.Repositories.SectionServicesRepo;
//using EMO.Repositories.SensorApplianceServicesRepo;
//using EMO.Repositories.SensorCommandRepo;
//using EMO.Repositories.SensorsChainRepo;
//using EMO.Repositories.SensorServicesRepo;
//using EMO.Repositories.SingalPhaseDataRepo;
//using EMO.Repositories.SingalPhaseDataServicesRepo;
//using EMO.Repositories.SubUserTypeServicesRepo;
//using EMO.Repositories.SuperAdminDashboardServicesRepo;
//using EMO.Repositories.TenantServicesRepo;
//using EMO.Repositories.UserServicesRepo;
//using EMO.Repositories.UserTypeServicesRepo;
//using EMO.Repositories.UtilityServicesRepo;

//using EnergyMonitor.Services;
//using Microsoft.EntityFrameworkCore;
//using StackExchange.Redis;

//namespace EMO.Extensions
//{
//    public static class ServiceExtension
//    {
//        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
//        {
//            services.AddDbContext<DBUserManagementContext>(options =>
//            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

//            services.Configure<RedisKeys>(
//            configuration.GetSection("RedisKeys"));

//            services.AddSingleton<IConnectionMultiplexer>(sp =>
//            {
//                var redisConnection =
//                    configuration.GetConnectionString("Redis");

//                return ConnectionMultiplexer.Connect(redisConnection);
//            });

//            //services.AddDbContext<DBUserManagementContext>(options =>
//            //options.UseMySql(
//            //    configuration.GetConnectionString("DefaultConnection"),
//            //    new MySqlServerVersion(new Version(8, 0, 32)) // replace with your MySQL version
//            //));
//            // Adding Cors
//            services.AddCors(options =>
//            {
//                options.AddPolicy(name: "AllowedOrigins",
//                                  policy =>
//                                  {
//                                      policy
//                                      .AllowAnyHeader()
//                                      .AllowAnyOrigin()
//                                      .AllowAnyMethod();
//                                  });
//            });
//            services.AddHostedService<SensorRedisStartupCacheService>();
//            services.AddHostedService<DeviceMacRedisStartupService>();
//            services.AddHostedService<ApplianceRedisStartupCacheService>();
//            services.AddHostedService<HvacLoopRedisStartupCacheService>();
//            services.AddHostedService<DashboardAggregateWorker>();
//            services.AddAutoMapper(typeof(Program).Assembly);
//            services.AddTransient<IJWTUtils, JWTUtils>();
//            services.AddTransient<IApiKeyService, ApiKeyService>();
//            services.AddScoped<ISensorRedisCacheService, SensorRedisCacheService>();
//            services.AddScoped<IDeviceRedisService, DeviceRedisService>();
//            services.AddSingleton<UserAuthorizeAttribute>();
//            services.AddSingleton<ApiKeyAttribute>();
//            services.AddTransient<OtherServices>();
//            services.AddHttpClient();
//            services.AddScoped<ISensorCommandService, SensorCommandService>();
//            services.AddTransient<IUserServices, UserServices>();
//            services.AddTransient<IAuthServices, AuthServices>();
//            services.AddTransient<IInnerServices, InnerServices>();
//            services.AddTransient<IUserTypeService, UserTypeService>();
//            services.AddTransient<IBusinessServices, BusinessServices>();
//            services.AddTransient<IBuildingServices, BuildingServices>();
//            services.AddTransient<DashboardService>();
//            services.AddScoped<IEnergyDashboardService, EnergyDashboardService>();
//            services.AddTransient<IFacilityServices, FacilityServices>();
//            services.AddTransient<ISectionServices, SectionServices>();
//            services.AddTransient<IFloorServices, FloorServices>();
//            services.AddTransient<IOfficeServices, OfficeServices>();
//            //services.AddTransient<IControlTypeServices, ControlTypeServices>();
//            services.AddTransient<IDeviceServices, DeviceServices>();
//            //services.AddTransient<IDeviceTypeServices, DeviceTypeServices>();
//            services.AddTransient<IContactPersonServices, ContactPersonServices>();
//            services.AddTransient<ISingalPhaseDataService, SingalPhaseDataService>();
//            services.AddTransient<ITenantServices, TenantServices>();
//            services.AddTransient<IUtilityServices, UtilityServices>();
//            services.AddTransient<ISuperAdminDashboardServices, SuperAdminDashboardServices>();
//            services.AddTransient<IAgreementServices, AgreementServices>();
//            services.AddTransient<IGenderServices, GenderServices>();
//            services.AddTransient<ISubUserTypeServices, SubUserTypeServices>();
//            services.AddTransient<ISensorServices, SensorServices>();
//            services.AddTransient<IApplianceServices, ApplianceServices>();
//            services.AddTransient<ISensorApplianceServices, SensorApplianceServices>();
//            services.AddTransient<IHvacLoopSettingServices, HvacLoopSettingServices>();
//            services.AddScoped<IApplianceRedisCacheService, ApplianceRedisCacheService>();
//            services.AddScoped<IHvacLoopRedisCacheService, HvacLoopRedisCacheService>();
//            services.AddScoped<IOptimizationDashboardService, OptimizationDashboardService>();
//        }
//    }
//}
using EMO.Extensions.MiddleWare;
using EMO.Models.DBModels;
using EMO.Repositories.AgreementServicesRepo;
using EMO.Repositories.ApiKeyServiceRepo;
using EMO.Repositories.ApplianceRedisRepo;
using EMO.Repositories.ApplianceServicesRepo;
using EMO.Repositories.AuthServicesRepo;
using EMO.Repositories.BuildingServicesRepo;
using EMO.Repositories.BusinessServicesRepo;
using EMO.Repositories.ContactPersonServicesRepo;
using EMO.Repositories.DashboardServicesRepo;
using EMO.Repositories.DeepDiveRepo;
using EMO.Repositories.DemandManagementRedisRepo;
using EMO.Repositories.DeviceRedisRepo;
using EMO.Repositories.DeviceServicesRepo;
using EMO.Repositories.EnergyConfigurationRepo;
using EMO.Repositories.EnergyDashboardRepo;
using EMO.Repositories.EnergyDashboardServicesRepo;
using EMO.Repositories.HistoricalDataRepo;
using EMO.Repositories.FacilityServicesRepo;
using EMO.Repositories.FloorServicesRepo;
using EMO.Repositories.GenderServicesRepo;
using EMO.Repositories.HvacLoopRedisRepo;
using EMO.Repositories.HvacLoopSettingServicesRepo;
using EMO.Repositories.InnerServicesRepo;
using EMO.Repositories.JWTUtilsRepo;
using EMO.Repositories.OfficeServicesRepo;
using EMO.Repositories.OptimizationDashboardRepo;
using EMO.Repositories.OptimizationRuntimeRepo;
using EMO.Repositories.RedisStartupService;
using EMO.Repositories.SectionServicesRepo;
using EMO.Repositories.SensorApplianceServicesRepo;
using EMO.Repositories.SensorCommandRepo;
using EMO.Repositories.SensorsChainRepo;
using EMO.Repositories.SensorServicesRepo;
using EMO.Repositories.SingalPhaseDataRepo;
using EMO.Repositories.SingalPhaseDataServicesRepo;
using EMO.Repositories.SubUserTypeServicesRepo;
using EMO.Repositories.SuperAdminDashboardServicesRepo;
using EMO.Repositories.TenantServicesRepo;
using EMO.Repositories.UserServicesRepo;
using EMO.Repositories.UserTypeServicesRepo;
using EMO.Repositories.UtilityServicesRepo;

using EnergyMonitor.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace EMO.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DBUserManagementContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<RedisKeys>(
            configuration.GetSection("RedisKeys"));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection =
                    configuration.GetConnectionString("Redis");

                return ConnectionMultiplexer.Connect(redisConnection);
            });

            //services.AddDbContext<DBUserManagementContext>(options =>
            //options.UseMySql(
            //    configuration.GetConnectionString("DefaultConnection"),
            //    new MySqlServerVersion(new Version(8, 0, 32)) // replace with your MySQL version
            //));
            // Adding Cors
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowedOrigins",
                                  policy =>
                                  {
                                      policy
                                      .AllowAnyHeader()
                                      .AllowAnyOrigin()
                                      .AllowAnyMethod();
                                  });
            });
            services.AddHostedService<SensorRedisStartupCacheService>();
            services.AddHostedService<DeviceMacRedisStartupService>();
            services.AddHostedService<ApplianceRedisStartupCacheService>();
            services.AddHostedService<HvacLoopRedisStartupCacheService>();
            services.AddHostedService<DemandManagementRedisStartupCacheService>();
            services.AddHostedService<DashboardAggregateWorker>();
            services.AddAutoMapper(typeof(Program).Assembly);
            services.AddTransient<IJWTUtils, JWTUtils>();
            services.AddTransient<IApiKeyService, ApiKeyService>();
            services.AddScoped<ISensorRedisCacheService, SensorRedisCacheService>();
            services.AddScoped<IDeviceRedisService, DeviceRedisService>();
            services.AddSingleton<UserAuthorizeAttribute>();
            services.AddSingleton<ApiKeyAttribute>();
            services.AddTransient<OtherServices>();
            services.AddHttpClient();
            services.AddScoped<ISensorCommandService, SensorCommandService>();
            services.AddTransient<IUserServices, UserServices>();
            services.AddTransient<IAuthServices, AuthServices>();
            services.AddTransient<IInnerServices, InnerServices>();
            services.AddTransient<IUserTypeService, UserTypeService>();
            services.AddTransient<IBusinessServices, BusinessServices>();
            services.AddTransient<IBuildingServices, BuildingServices>();
            services.AddTransient<DashboardService>();
            services.AddScoped<IEnergyDashboardService, EnergyDashboardService>();
            services.AddScoped<IHistoricalDataService, HistoricalDataService>();
            services.AddTransient<IFacilityServices, FacilityServices>();
            services.AddTransient<ISectionServices, SectionServices>();
            services.AddTransient<IFloorServices, FloorServices>();
            services.AddTransient<IOfficeServices, OfficeServices>();
            //services.AddTransient<IControlTypeServices, ControlTypeServices>();
            services.AddTransient<IDeviceServices, DeviceServices>();
            //services.AddTransient<IDeviceTypeServices, DeviceTypeServices>();
            services.AddTransient<IContactPersonServices, ContactPersonServices>();
            services.AddTransient<ISingalPhaseDataService, SingalPhaseDataService>();
            services.AddTransient<ITenantServices, TenantServices>();
            services.AddTransient<IUtilityServices, UtilityServices>();
            services.AddTransient<ISuperAdminDashboardServices, SuperAdminDashboardServices>();
            services.AddTransient<IAgreementServices, AgreementServices>();
            services.AddTransient<IGenderServices, GenderServices>();
            services.AddTransient<ISubUserTypeServices, SubUserTypeServices>();
            services.AddTransient<ISensorServices, SensorServices>();
            services.AddTransient<IApplianceServices, ApplianceServices>();
            services.AddTransient<ISensorApplianceServices, SensorApplianceServices>();
            services.AddTransient<IHvacLoopSettingServices, HvacLoopSettingServices>();
            services.AddScoped<IApplianceRedisCacheService, ApplianceRedisCacheService>();
            services.AddScoped<IHvacLoopRedisCacheService, HvacLoopRedisCacheService>();
            services.AddScoped<IDemandManagementRedisCacheService, DemandManagementRedisCacheService>();
            services.AddScoped<IOptimizationDashboardService, OptimizationDashboardService>();
            services.AddScoped<IOptimizationRuntimeService, OptimizationRuntimeService>();
            services.AddScoped<IEnergyConfigurationService, EnergyConfigurationService>();
            services.AddScoped<ISensorEnergyAggregateStore, SensorEnergyAggregateStore>();
            services.AddScoped<IDeepDiveService, DeepDiveService>();
        }
    }
}
