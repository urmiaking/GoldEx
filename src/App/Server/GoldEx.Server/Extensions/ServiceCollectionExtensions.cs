using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Server.Api.Identity;
using GoldEx.Server.Infrastructure.Services;
using GoldEx.Server.Services;
using GoldEx.Shared.Routings;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using GoldEx.Shared.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GoldEx.Server.Infrastructure.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddControllers(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        return services;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddDatabaseDeveloperPageExceptionFilter();
        services.DiscoverServices();

        services.AddSingleton<IEmailSender<AppUser>, EmailSender>();

        return services;
    }

    internal static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    internal static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var globalSettings = TypeAdapterConfig.GlobalSettings;

        globalSettings.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(globalSettings);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    internal static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();

        return services;
    }
    
    internal static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
        services.Configure<SmsSettings>(configuration.GetSection(nameof(SmsSettings)));

        return services;
    }

    internal static IServiceCollection AddComponents(this IServiceCollection services)
    {
        services
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        return services;
    }

    internal static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GoldEx");

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("GoldEx connection string is not available");

        services.AddSqlServer<GoldExDbContext>(connectionString);

        return services;
    }

    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddCascadingAuthenticationState();

        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];

        var isGoogleAuthConfigured = !string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret);

        if (isGoogleAuthConfigured)
        {
            services.AddAuthentication(o =>
                {
                    // This forces challenge results to be handled by Google OpenID Handler
                    o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
                    // This forces forbid results to be handled by Google OpenID Handler
                    o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
                    // Default scheme that will handle everything else. Use the Identity Cookie Scheme as the default.
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(GoldExSignInManager<AppUser>.GoldExScheme,
                    config =>
                    {
                        config.ExpireTimeSpan = TimeSpan.FromHours(1);
                    })
                .AddGoogleOpenIdConnect(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
        }
        else
        {
            services.AddAuthentication()
                .AddCookie(GoldExSignInManager<AppUser>.GoldExScheme, config => config.ExpireTimeSpan = TimeSpan.FromHours(1));
        }

        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<GoldExDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<GoldExSignInManager<AppUser>>();

        services.ConfigureApplicationCookie(config =>
        {
            config.ExpireTimeSpan = TimeSpan.FromDays(90);
            var defaultEvents = config.Events;

            config.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }

                    return defaultEvents.OnRedirectToLogin(ctx);
                },
                OnRedirectToAccessDenied = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }

                    return defaultEvents.OnRedirectToAccessDenied(ctx);
                }
            };
        });

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });
        services.ConfigureOptions<ConfigureSecurityStampOptions>();

        return services;
    }

    internal static IServiceCollection AddAppHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("GoldEx")!, healthQuery: "select 1",
                name: "SQL Database Server",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["Database"])
            .AddCheck<TalaIrHealthCheck>(
                name: "Tala.ir price fetcher api",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["External API"])
            .AddCheck<MemoryHealthCheck>(
                name:"Memory Check",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["Memory"]);

        services.AddHealthChecksUI(opt =>
            {
                opt.SetEvaluationTimeInSeconds(60); //time in seconds between check    
                opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks    
                opt.SetApiMaxActiveRequests(1); //api requests concurrency    
                opt.AddHealthCheckEndpoint("GoldEx Service Health Checker", ApiRoutes.Health.Base); //map health check api
            })
            .AddInMemoryStorage();

        return services;
    }
}