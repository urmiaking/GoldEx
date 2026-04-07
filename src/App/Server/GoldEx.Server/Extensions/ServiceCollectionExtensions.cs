using AspNetCore.DataProtection.SqlServer;
using DevExpress.AspNetCore;
using DevExpress.Drawing;
using GoldEx.Client.Components.Services;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Api.Identity;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.HealthChecks;
using GoldEx.Server.Infrastructure.Services;
using GoldEx.Server.Services;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Settings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.MsSqlServerProvider.Extensions;
using Serilog.Ui.Web.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoldEx.Sdk.Server.Application.Extensions;
using VHDLicenseManager;

namespace GoldEx.Server.Extensions;

internal static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddControllers(IConfiguration configuration)
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

        internal IServiceCollection AddServices()
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton<IEmailSender<AppUser>, IdentityEmailSender>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddUserContext();

            services.DiscoverServices();

            return services;
        }

        internal IServiceCollection AddSwagger()
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        internal IServiceCollection AddMapster()
        {
            var globalSettings = TypeAdapterConfig.GlobalSettings;

            globalSettings.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton(globalSettings);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        internal IServiceCollection AddCache()
        {
            services.AddMemoryCache();

            return services;
        }

        internal IServiceCollection AddSettings(IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.Configure<BackupSettings>(configuration.GetSection(nameof(BackupSettings)));
            services.Configure<SmsSettings>(configuration.GetSection(nameof(SmsSettings)));
            services.Configure<DefaultSetting>(configuration.GetSection(nameof(DefaultSetting)));
            services.Configure<UserSetting>(configuration.GetSection(nameof(UserSetting)));
            services.Configure<PriceProviderSetting>(configuration.GetSection(nameof(PriceProviderSetting)));

            return services;
        }

        internal IServiceCollection AddComponents()
        {
            services
                .AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            return services;
        }

        internal IServiceCollection AddStorage(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("GoldEx");

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("GoldEx connection string is not available");

            services.AddSqlServer<GoldExDbContext>(connectionString, options =>
                {
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    options.CommandTimeout(1800);
                },
                dbContextOptions =>
                {
                    dbContextOptions.ConfigureWarnings(w =>
                        w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
                });

            return services;
        }

        internal IServiceCollection AddDataProtectionStore(IConfiguration configuration)
        {
            services.AddDataProtection()
                .PersistKeysToSqlServer(connectionString: configuration.GetConnectionString("GoldEx"),
                    schema: "dbo",
                    table: "DataProtectionKeys")
                .SetApplicationName("GoldExSuite");

            return services;
        }

        internal IServiceCollection AddAuth(IConfiguration configuration)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddCascadingAuthenticationState();

            // 1. Setup Identity First. This sets up the default cookies (Identity.Application).
            services.AddIdentity<AppUser, AppRole>(options =>
                {
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddEntityFrameworkStores<GoldExDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager<GoldExSignInManager<AppUser>>();

            // 2. Configure External Providers (Google)
            // AddIdentity registers authentication services, so we can access the builder here.
            var authBuilder = services.AddAuthentication();

            var googleClientId = configuration["Authentication:Google:ClientId"];
            var googleClientSecret = configuration["Authentication:Google:ClientSecret"];

            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }

            // 3. Configure the Application Cookie (The one Identity created)
            // This handles Expiration AND the API Redirect logic in one place.
            services.ConfigureApplicationCookie(config =>
            {
                config.ExpireTimeSpan = TimeSpan.FromDays(90);
                config.SlidingExpiration = true;

                // Ensure we are configuring the scheme Identity uses
                // (Optional if you just want to use the default 'Identity.Application')
                // config.Cookie.Name = "GoldExToken"; 
                config.Cookie.Name = "GoldExAuthCookie";

                var defaultEvents = config.Events;

                config.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        // Check if it's an API call
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        return defaultEvents.OnRedirectToLogin(ctx);
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        // Check if it's an API call
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                        return defaultEvents.OnRedirectToAccessDenied(ctx);
                    }
                };
            });

            // 4. Configure Identity Options (Password requirements, etc.)
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureOptions<ConfigureSecurityStampOptions>();

            return services;
        }

        internal IServiceCollection AddAppHealthCheck(IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddSqlServer(configuration.GetConnectionString("GoldEx")!, healthQuery: "select 1",
                    name: "پایگاه داده",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["پایگاه داده"])
                .AddCheck<SignalHealthCheck>(
                    name: "سرویس استعلام قیمت",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["سرویس استعلام قیمت"])
                .AddCheck<MemoryHealthCheck>(
                    name:"عملکرد RAM",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["رم"]);

            return services;
        }

        internal IServiceCollection AddSerilogUiService(IConfiguration configuration)
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

        internal IServiceCollection AddDevExpress()
        {
            services.AddDevExpressControls();

            Settings.DrawingEngine = DrawingEngine.Skia;

            DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(typeof(GetInvoiceReportResponse).Assembly);
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceReportResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetPriceUnitTitleResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceProductItemResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetSettingResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceDiscountResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoicePaymentResponse));
            DevExpress.Utils.DeserializationSettings.RegisterTrustedClass(typeof(GetInvoiceExtraCostsResponse));

            return services;
        }

        internal IServiceCollection AddClientServices()
        {
            services.AddScoped<HelpContext>();
            services.AddScoped<LicenseState>();
            services.AddScoped<WebAuthnService>();

            return services;
        }

        internal IServiceCollection AddLicense()
        {
            services.AddSingleton<License>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var url = config["License:BaseUrl"];
                return new License(url);
            });

            services.AddSingleton<ProductLicense>();

            return services;
        }
    }

    // TODO: refactor
}