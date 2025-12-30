using Microsoft.EntityFrameworkCore;
using P3AHR.Repositories.JWTUtilsRepo;
using P3AHR.Repositories.ApiKeyServiceRepo;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DBModels;
using P3AHR.Repositories.UserServicesRepo;
using P3AHR.Repositories.AuthServicesRepo;
using P3AHR.Repositories.InnerServicesRepo;
using APIProduct.Repositories.UserTypeServicesRepo;
using APIProduct.Repositories.BusinessServicesRepo;
using APIProduct.Repositories.BuildingServicesRepo;
using APIProduct.Repositories.FacilityServicesRepo;
using APIProduct.Repositories.SectionServicesRepo;
using APIProduct.Repositories.FloorServicesRepo;
using APIProduct.Repositories.OfficeServicesRepo;
using APIProduct.Repositories.DeviceServicesRepo;
using APIProduct.Repositories.DeviceTypeServicesRepo;
using APIProduct.Repositories.PocServicesRepo;

namespace P3AHR.Extensions
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
