using BidX.DataAccess;
using BidX.DataAccess.Entites;
using BidX.Presentation.Utils;
using BidXesentation.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAndConfigureApiControllers()
    .AddAndConfigureSwagger()
    .AddAndConfigureDBContext()
    .AddAndConfigureIdentity()
    .AddAndConfigureJwtAuthentication()
    .AddAndConfigureSignalR()
    .AddAndConfigureCors(builder.Configuration)
    .AddApplicationServices();

// https://nblumhardt.com/2024/04/serilog-net8-0-minimal
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


var app = builder.Build();

// Apply Migrations that hasn't been applied and seed roles and admin users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var appDbContext = services.GetRequiredService<AppDbContext>();
        await appDbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await roleManager.SeedRoles();

        var userManager = services.GetRequiredService<UserManager<User>>();
        await userManager.SeedAdminAccounts();

        await appDbContext.SeedCities();
        await appDbContext.SeedCategories();
    }
    catch (Exception ex)
    {
        Log.Error(ex.ToString());
    }
}

app.UseExceptionHandler(o => { }); // i added o=>{} due to a bug in .NET8 see this issue for more info ttps://github.com/dotnet/aspnetcore/issues/51888

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontendDomain");

app.UseAuthentication(); // Validates the Token came at the request's Authorization header then decode it and assign it to HttpContext.User

app.UseAuthorization();

app.MapControllers();

app.MapHub<Hub>("/hub");

app.Run();
