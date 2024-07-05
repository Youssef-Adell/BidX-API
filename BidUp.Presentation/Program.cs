using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using BidUp.BusinessLogic;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.BusinessLogic.Services;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using BidUp.Presentation.Hubs;
using BidUp.Presentation.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // To serialize enum values to string instead of int
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter()); // To serialize and deserailize the date time in UTC format (https://github.com/dotnet/runtime/issues/1566)
                });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Modify the default behaviour of [APIController] Attribute to return a customized error response instead of the default response to unify error responses accross the api
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressMapClientErrors = true; //https://stackoverflow.com/a/56377973
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var validationErrorMessages = actionContext.ModelState.Values
            .Where(stateEntry => stateEntry.Errors.Count > 0)
            .SelectMany(stateEntry => stateEntry.Errors)
            .Select(error => error.ErrorMessage);

        var errorResponse = new ErrorResponse(ErrorCode.USER_INPUT_INVALID, validationErrorMessages);

        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BidUp",
        Description = "A real-time API for an online auction/bidding system written in ASP.NET Core.",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Youssef Adel",
            Email = "YoussefAdel.Fci@gmail.com"
        },
    });

    // makes Swagger-UI renders the "Authorize" button which when clicked brings up the Authorize dialog box
    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    //to remove the lock symbol from the endpoints that doesnt has [authorize] attribute
    options.OperationFilter<SecurityRequirementsOperationFilter>(false, "bearerAuth"); // SecurityRequirementsOperationFilter has a constructor that accepts securitySchemaName and i can pass arguments to it via the OperationFilter method, so i passed the name of the SecurityScheme defined above (bearerAuth) otherwise it wont send the authorization header with the requests

    //to make swagger shows the the docs comments and responses for the endpoints 
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

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

    // The access token is sent in the query string when SignalR uses WebSockets or ServerSentEvents due to a limitation in Browser APIs.
    // So I have to hook the OnMessageReceived event in order to allow the JWT authentication handler to read the access token
    // from the query string when a WebSocket or Server-Sent Events request comes in. (for more: https://youtu.be/mrCxfifTepU?si=Zj6Ghx0aZEVFELPL, https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz).
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Read the token from the query string for requests that come to our SignalR hub only
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/appHub"))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// https://nblumhardt.com/2024/04/serilog-net8-0-minimal
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    // To be able to test signalR hub using JS client
    options.AddPolicy(name: "DevelopmentPolicy", policy => policy.WithOrigins("http://127.0.0.1:5500").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, BrevoEmailService>();
builder.Services.AddScoped<ICitiesService, CitiesServices>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<ICloudService, CloudinaryCloudService>();
builder.Services.AddScoped<IAuctionsService, AuctionsService>();
builder.Services.AddScoped<IBiddingService, BiddingService>();
builder.Services.AddScoped<IUsersService, UsersService>();


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

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}

app.UseAuthentication(); // Validates the Token came at the request's Authorization header then decode it and assign it to HttpContext.User

app.UseAuthorization();

app.MapControllers();

app.MapHub<AppHub>("/appHub");

app.Run();
