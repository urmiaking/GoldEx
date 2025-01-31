﻿using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Components.Account;
using GoldEx.Server.Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Server.Api.Identity;
using GoldEx.Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace GoldEx.Server.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection InitializeDefaultCulture(this IServiceCollection services)
    {
        var culture = new CultureInfo("fa-IR")
        {
            NumberFormat =
            {
                NegativeSign = "-",
                NumberDecimalSeparator = "."
            },
            DateTimeFormat =
            {
                ShortDatePattern = "yyyy/MM/dd",
                FirstDayOfWeek = DayOfWeek.Saturday,
                ShortestDayNames = ["ی", "د", "س", "چ", "پ", "ج", "ش"],
                MonthNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty]
            }
        };

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        return services;
    }

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
        //services.AddSharedServices();

        services.AddSingleton<IEmailSender<AppUser>, IdentityNoOpEmailSender>();

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
        services.AddSqlServer<GoldExDbContext>(configuration.GetConnectionString("GoldEx"));

        return services;
    }

    internal static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddCascadingAuthenticationState();

        services.AddAuthentication()
            .AddCookie(GoldExSignInManager<AppUser>.GoldExScheme,
                       config =>
                       {
                           config.ExpireTimeSpan = TimeSpan.FromHours(1);
                       });

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
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        });
        services.ConfigureOptions<ConfigureSecurityStampOptions>();

        return services;
    }
}