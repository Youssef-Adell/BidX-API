using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Services;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace BidX.Presentation.Utils;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAndConfigureApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // To serialize enum values to string instead of int
                });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Modify the default behaviour of [APIController] Attribute to return a customized error response instead of the default response to unify error responses accross the api
        services.Configure<ApiBehaviorOptions>(options =>
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

        services.AddEndpointsApiExplorer();
        
        return services;
    }

    public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BidX",
                Description = "A real-time API for an online auction/bidding platform written in ASP.NET Core.",
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
            options.OperationFilter<SecurityRequirementsOperationFilter>(false, "bearerAuth");

            //to make swagger shows the the docs comments and responses for the endpoints 
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }

    public static IServiceCollection AddAndConfigureDBContext(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(Environment.GetEnvironmentVariable("BIDX_DB_CONNECTION_STRING")));
        
        return services;
    }

    public static IServiceCollection AddAndConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<int>>(options =>
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

        return services;
    }

    public static IServiceCollection AddAndConfigureJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(static options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BIDX_JWT_SECRET_KEY")!)),
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/appHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
    
    public static IServiceCollection AddAndConfigureSignalR(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(10); // 2X the KeepAliveInterval value configured by the client
        });

        return services;
    }

    public static IServiceCollection AddAndConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var frontendOrigin = configuration["Cors:FrontendOrigin"]!;
            options.AddPolicy(name: "AllowFrontendDomain", policy => 
                policy.WithOrigins(frontendOrigin)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, BrevoEmailService>();
        services.AddScoped<ICitiesService, CitiesServices>();
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<ICloudService, CloudinaryCloudService>();
        services.AddScoped<IAuctionsService, AuctionsService>();
        services.AddScoped<IBidsService, BidsService>();
        services.AddScoped<IProfilesService, ProfilesService>();
        services.AddScoped<IChatsService, ChatsService>();
        services.AddScoped<IReviewsService, ReviewsService>();

        return services;
    }
}