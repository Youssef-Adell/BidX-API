using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Services;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Modify the default behaviour of [APIController] Attribute to return a customized error response instead of the default response to unify error responses accross the api
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var validationErrors = actionContext.ModelState.Values
            .Where(stateEntry => stateEntry.Errors.Count > 0)
            .SelectMany(stateEntry => stateEntry.Errors);

        var errorResponse = new ErrorResponse(ErrorCode.USER_INPUT_INVALID_SYNTAX, validationErrors.Humanize(e => e.ErrorMessage));

        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Environment.GetEnvironmentVariable("BIDUP_DB_CONNECTION_STRING")));

// Identity (Add and configure Services To Manage, Create and Validate Users)
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();



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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
