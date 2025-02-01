using GoldEx.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace GoldEx.Client;

internal static class DependencyInjection
{
    internal static IServiceCollection AddClient(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        services.InitializeDefaultCulture()
            .AddClient()
            .AddAuthServices()
            .AddServices()
            .AddJsonOptions()
            .AddHttpClientService(environment);

        return services;
    }
}