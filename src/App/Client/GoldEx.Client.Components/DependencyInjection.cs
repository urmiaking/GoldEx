using Blazored.LocalStorage;
using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Components.Services.Abstractions;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Globalization;

namespace GoldEx.Client.Components;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClientComponents()
        {
            services.AddScoped<HelpContext>();
            services.AddScoped<LicenseState>();
            services.AddScoped<WebAuthnService>();

            return services.DiscoverServices();
        }

        public IServiceCollection InitializeDefaultCulture()
        {
            var culture = new CultureInfo("fa-IR")
            {
                NumberFormat =
                {
                    NegativeSign = "-",
                    NumberDecimalSeparator = ".",
                    CurrencySymbol = "ریال",
                    CurrencyPositivePattern = 3, // Symbol on the left with a space  
                    CurrencyNegativePattern = 8, // Symbol on the left with a space for negative values  
                    NumberGroupSeparator = ",",     // Explicitly set this
                    CurrencyDecimalSeparator = ".", // Explicitly set this (was defaulting to /)
                    CurrencyGroupSeparator = ",",   // Explicitly set this
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
                    AbbreviatedMonthGenitiveNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty],
                    AbbreviatedMonthNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty]
                }
            };

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            return services;
        }

        public IServiceCollection AddClientAndServerServices(string snackBarPositionClass)
        {
            services.AddBlazoredLocalStorage();

            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = snackBarPositionClass;
                config.SnackbarConfiguration.HideTransitionDuration = 1500;
                config.PopoverOptions.Duration = TimeSpan.FromMilliseconds(500);
            });

            services.AddLocalization();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<IVersionReleaseStore, VersionReleaseStore>();

            return services;
        }

        public IServiceCollection AddJsonOptions()
        {
            var jsonOptions = Shared.Utilities.GetJsonOptions();
            services.AddSingleton(jsonOptions);

            return services;
        }

        public IServiceCollection AddHttpClientService(IWebAssemblyHostEnvironment environment, TimeSpan timeOut)
        {
            var baseAddress = environment.BaseAddress;
            services.AddScoped(_ =>
            {
                var client = Shared.Utilities.GetHttpClient(baseAddress);
                client.Timeout = timeOut;
                return client;
            });

            return services;
        }

        public IServiceCollection AddAuthServices()
        {
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

            return services;
        }
    }
}