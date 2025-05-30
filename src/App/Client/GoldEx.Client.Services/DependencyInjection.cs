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
        services.AddScoped<ISettingHttpClientService, SettingHttpClientService>();

        services.AddScoped<IPriceService, PriceHttpClientService>();
        services.AddScoped<IHealthService, HealthService>();

        services.DiscoverServices();

        return services;
    }
}