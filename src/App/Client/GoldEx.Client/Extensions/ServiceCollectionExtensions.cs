using GoldEx.Sdk.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;
using System.Runtime.InteropServices;
using Blazored.LocalStorage;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Offline.Infrastructure;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Shared;
using MudBlazor.Services;
using GoldEx.Client.Services;
using GoldEx.Sdk.Client.Abstractions;
using MudBlazor;
using GoldEx.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection InitializeDefaultCulture(this IServiceCollection services)
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
                LongDatePattern = "dddd, dd MMMM yyyy",
                FirstDayOfWeek = DayOfWeek.Saturday,
                ShortestDayNames = ["ی", "د", "س", "چ", "پ", "ج", "ش"],
                DayNames = ["یکشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"],
                MonthNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty],
                MonthGenitiveNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty],
                AbbreviatedMonthGenitiveNames = ["فرو", "ارد", "خرد", "تیر", "مرد", "شهر", "مهر", "آبا", "آذر", "دی", "بهم", "اسف", string.Empty],
                AbbreviatedMonthNames = ["فرو", "ارد", "خرد", "تیر", "مرد", "شهر", "مهر", "آبا", "آذر", "دی", "بهم", "اسف", string.Empty]
            }
        };

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        return services;
    }

    internal static IServiceCollection AddAuthServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        builder.AddAuthorizationCore();
        builder.AddCascadingAuthenticationState();
        builder.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

        return builder;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.DiscoverServices();
        services.AddScoped<IPriceClientService, PriceClientService>();
        services.AddScoped<IHealthClientService, HealthClientService>();
        services.AddScoped<IImageClientService, ImageClientService>();
        services.AddScoped<IProductClientService, ProductClientService>();

        return services;
    }

    internal static IServiceCollection AddJsonOptions(this IServiceCollection services)
    {
        var jsonOptions = Utilities.GetJsonOptions();
        services.AddSingleton(jsonOptions);

        return services;
    }

    internal static IServiceCollection AddHttpClientService(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        var baseAddress = environment.BaseAddress;
        services.AddScoped(sp =>
        {
            var client = Utilities.GetHttpClient(baseAddress);
            return client;
        });

        return services;
    }

    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
            services.AddClientOnlyServices();

        services.AddClientAndServerServices();

        return services;
    }

    public static IServiceCollection AddClientOnlyServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddClientAndServerServices(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();

        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        });

        services.AddLocalization();
        services.AddScoped<IBusyIndicator, BusyIndicator>();

        return services;
    }
}