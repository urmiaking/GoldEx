﻿using Blazored.LocalStorage;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Services;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Shared;
using GoldEx.Shared.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;
using System.Runtime.InteropServices;

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
                NumberDecimalSeparator = ".",
                CurrencySymbol = "ریال",
                CurrencyPositivePattern = 3, // Symbol on the left with a space  
                CurrencyNegativePattern = 8 // Symbol on the left with a space for negative values  
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

    public static IServiceCollection AddClientServerServices(this IServiceCollection services)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
            services.AddClientOnlyServices();

        services.AddClientAndServerServices();

        return services;
    }

    public static IServiceCollection AddClientOnlyServices(this IServiceCollection services)
    {
        services.AddClientServices();

        return services;
    }

    public static IServiceCollection AddClientAndServerServices(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();

        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.HideTransitionDuration = 1500;
        });

        services.AddLocalization();
        services.AddScoped<IThemeService, ThemeService>();

        return services;
    }
}