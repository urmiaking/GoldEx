using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Services.ClientServices;
using GoldEx.Client.Services.HttpServices;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Client.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        services.AddScoped<IProductHttpClientService, ProductHttpClientService>();
        services.AddScoped<ISettingsHttpClientService, SettingsHttpClientService>();

        services.AddScoped<IPriceClientService, PriceHttpClientService>();
        services.AddScoped<IHealthClientService, HealthClientService>();
        services.AddScoped<IProductClientService, ProductClientService>();

        services.DiscoverServices();

        return services;
    }
}