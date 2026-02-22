using GoldEx.Calculator.Client.Services;
using GoldEx.Client.Components;
using GoldEx.Client.Services;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Runtime.InteropServices;

namespace GoldEx.Calculator.Client;

public static class DependencyInjection
{
    internal static IServiceCollection AddClient(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        services.InitializeDefaultCulture()
            .AddClientComponents()
            .AddClientServerServices()
            .AddAuthServices()
            .AddServices()
            .AddJsonOptions()
            .AddHttpClientService(environment);

        return services;
    }

    public static IServiceCollection AddClientServerServices(this IServiceCollection services)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
            services.AddClientServices();

        services.AddClientAndServerServices();
        services.AddScoped<QuickInvoiceBasketStore>();

        return services;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.DiscoverServices();

        return services;
    }
}