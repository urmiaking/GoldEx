using GoldEx.Client.Components.Services;
using GoldEx.Client.Extensions;
using GoldEx.Client.Offline;
using GoldEx.Shared.Abstractions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace GoldEx.Client;

internal static class DependencyInjection
{
    internal static IServiceCollection AddClient(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        services.InitializeDefaultCulture()
            .AddClientServices()
            .AddAuthServices()
            .AddServices()
            .AddJsonOptions()
            .AddHttpClientService(environment)
            .AddOfflineClient();

        services.AddScoped<IThemeService, ThemeService>();

        return services;
    }
}