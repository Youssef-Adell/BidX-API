using System.Text;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Services;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using BidUp.Presentation.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Modify the default behaviour of [APIController] Attribute to return a customized error response instead of the default response to unify error responses accross the api
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var validationErrorMessages = actionContext.ModelState.Values
            .Where(stateEntry => stateEntry.Errors.Count > 0)
            .SelectMany(stateEntry => stateEntry.Errors)
            .Select(error => error.ErrorMessage);

        var errorResponse = new ErrorResponse(ErrorCode.USER_INPUT_INVALID_SYNTAX, string.Join("\n", validationErrorMessages));

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

// Authentication (Add and configure Services required by Authentication middleware To Validate the Token came with the request)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BIDUP_JWT_SECRET_KEY")!)),
        ClockSkew = TimeSpan.FromSeconds(30), //if we diidnt change its default value the token will be still valid for additional 5 minutes after its expiration time https://www.youtube.com/watch?v=meBxWjA_2YY
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, BrevoEmailService>();


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

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler(o => { }); // i added o=>{} due to a bug in .NET8 see this issue for more info ttps://github.com/dotnet/aspnetcore/issues/51888

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Validates the Token came at the request's Authorization header then decode it and assign it to HttpContext.User

app.UseAuthorization();

app.MapControllers();

app.Run();
