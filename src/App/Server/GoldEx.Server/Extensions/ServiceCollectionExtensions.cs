using DevExpress.AspNetCore;
using DevExpress.Drawing.Internal;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Api.Identity;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.HealthChecks;
using GoldEx.Server.Infrastructure.Services;
using GoldEx.Server.Services;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.MsSqlServerProvider.Extensions;
using Serilog.Ui.Web.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevExpress.Drawing;

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

        services.AddSingleton<IEmailSender<AppUser>, IdentityEmailSender>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddHttpContextAccessor();

        services.DiscoverServices();

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
        services.Configure<DefaultSetting>(configuration.GetSection(nameof(DefaultSetting)));
        services.Configure<UserSetting>(configuration.GetSection(nameof(UserSetting)));

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
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie(GoldExSignInManager<AppUser>.GoldExScheme,
                    config =>
                    {
                        config.ExpireTimeSpan = TimeSpan.FromHours(1);
                    })
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId!;
                    options.ClientSecret = googleClientSecret!;
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
            .AddCheck<SignalHealthCheck>(
                name: "Signal price fetcher api",
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

    internal static IServiceCollection AddSerilogUiService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilogUi(opts =>
        {
            opts.UseSqlServer(options =>
            {
                options.WithConnectionString(configuration.GetConnectionString("GoldEx")!)
                    .WithTable("Logs");
            });
        });

        return services;
    }

    internal static IServiceCollection AddDevExpress(this IServiceCollection services)
    {
        services.AddDevExpressControls();

        DXDrawingEngine.ForceSkia();

        DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(typeof(GetInvoiceReportResponse).Assembly);
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceReportResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetPriceUnitTitleResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceItemResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetSettingResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceDiscountResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoicePaymentResponse));
        DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceExtraCostsResponse));

        return services;
    }
}