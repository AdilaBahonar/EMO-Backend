using Microsoft.EntityFrameworkCore;
using EMO.Repositories.JWTUtilsRepo;
using EMO.Repositories.ApiKeyServiceRepo;
using EMO.Extensions.MiddleWare;
using EMO.Models.DBModels;
using EMO.Repositories.UserServicesRepo;
using EMO.Repositories.AuthServicesRepo;
using EMO.Repositories.InnerServicesRepo;
using EMO.Repositories.UserTypeServicesRepo;
using EMO.Repositories.BusinessServicesRepo;
using EMO.Repositories.BuildingServicesRepo;
using EMO.Repositories.FacilityServicesRepo;
using EMO.Repositories.SectionServicesRepo;
using EMO.Repositories.FloorServicesRepo;
using EMO.Repositories.OfficeServicesRepo;
using EMO.Repositories.DeviceServicesRepo;
using EMO.Repositories.DeviceTypeServicesRepo;
using EMO.Repositories.PocServicesRepo;

namespace EMO.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            /* services.AddDbContext<DBUserManagementContext>(options =>
             options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));*/

            services.AddDbContext<DBUserManagementContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 32)) // replace with your MySQL version
            ));
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
            services.AddAutoMapper(typeof(Program).Assembly);
            services.AddTransient<IJWTUtils, JWTUtils>();
            services.AddTransient<IApiKeyService, ApiKeyService>();
            services.AddSingleton<UserAuthorizeAttribute>();
            services.AddSingleton<ApiKeyAttribute>();
            services.AddTransient<OtherServices>();
            services.AddTransient<IUserServices, UserServices>();
            services.AddTransient<IAuthServices, AuthServices>();
            services.AddTransient<IInnerServices, InnerServices>();
            services.AddTransient<IUserTypeService, UserTypeService>();
            services.AddTransient<IBusinessServices, BusinessServices>();
            services.AddTransient<IBuildingServices, BuildingServices>();
            services.AddTransient<IFacilityServices, FacilityServices>();
            services.AddTransient<ISectionServices, SectionServices>();
            services.AddTransient<IFloorServices, FloorServices>();
            services.AddTransient<IOfficeServices, OfficeServices>();
            services.AddTransient<IControlTypeServices, ControlTypeServices>();
            services.AddTransient<IDeviceServices, DeviceServices>();
            services.AddTransient<IDeviceTypeServices, DeviceTypeServices>();
            services.AddTransient<IPocServices, PocServices>();
        }
    }
}
