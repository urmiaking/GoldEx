using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Server.Application.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHostedService<CoinPriceBackgroundService>();
        services.AddHostedService<CurrencyPriceBackgroundService>();
        services.AddHostedService<GoldPriceBackgroundService>();

        services.DiscoverServices();
        return services;
    }
}