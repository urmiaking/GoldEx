using GoldEx.Calculator.Client.Services;
using GoldEx.Client.Components;
using GoldEx.Client.Services;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using System.Runtime.InteropServices;

namespace GoldEx.Calculator.Client;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddClient(IWebAssemblyHostEnvironment environment)
        {
            services.InitializeDefaultCulture()
                .AddClientComponents()
                .AddClientServerServices()
                .AddAuthServices()
                .AddServices()
                .AddJsonOptions()
                .AddHttpClientService(environment, new TimeSpan(0, 0, 30));

            return services;
        }

        public IServiceCollection AddClientServerServices()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
                services.AddClientServices();

            services.AddClientAndServerServices(Defaults.Classes.Position.BottomRight);
            services.AddScoped<QuickInvoiceBasketStore>();

            return services;
        }

        internal IServiceCollection AddServices()
        {
            services.DiscoverServices();

            return services;
        }
    }
}