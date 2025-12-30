using Microsoft.EntityFrameworkCore;
using P3AHR.Extensions;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DBModels;
using P3AHR.Repositories.AuthServicesRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.ConfigureServices(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddSwaggerGen();
builder.Services.ConfigureJWT(builder.Configuration);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    using (var ctx = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>())
    {
        if (ctx.Database.GetPendingMigrations().Any())
        {
            await ctx.Database.MigrateAsync();
        }
        await ctx.Database.EnsureCreatedAsync();
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.UseMiddleware<JWTMiddleWare>();
app.MapControllers();
app.Run();
