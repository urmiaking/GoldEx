using GoldEx.Client.Components;
using GoldEx.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Runtime.InteropServices;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;

namespace GoldEx.Calculator.Client;

internal static class DependencyInjection
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

    internal static IServiceCollection AddClientServerServices(this IServiceCollection services)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
            services.AddClientServices();

        services.AddClientAndServerServices();

        return services;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.DiscoverServices();

        return services;
    }
}