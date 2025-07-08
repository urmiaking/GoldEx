using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Services.Price;
using GoldEx.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient("TalaIrApi");
        services.AddScoped<IPriceFetcher, TalaIrPriceFetcher>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("TalaIrApi");
            return new TalaIrPriceFetcher(httpClient);
        });

        services.AddHttpClient("SignalApi");

        services.AddScoped<IPriceFetcher, SignalPriceFetcher>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("SignalApi");
            return new SignalPriceFetcher(httpClient, Utilities.GetJsonOptions());
        });

        services.DiscoverServices();

        return services;
    }
}