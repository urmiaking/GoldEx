using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Services.Price;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GoldEx.Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddHttpClient("PriceApi");

        services.AddScoped<IPriceFetcher, PriceFetcher>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("PriceApi");
            return new PriceFetcher(httpClient);
        });

        services.DiscoverServices();
        return services;
    }
}