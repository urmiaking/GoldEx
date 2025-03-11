using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Services.Price;
using GoldEx.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.SettingsAggregate;
using GoldEx.Shared.Infrastructure.Repositories;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

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

        ///////////////////

        services.AddScoped<IProductRepository<Product>, ProductRepository<Product>>();
        services.AddScoped<IPriceRepository<Price, PriceHistory>, PriceRepository<Price, PriceHistory>>();
        services.AddScoped<ISettingsRepository<Settings>, SettingsRepository<Settings>>();

        ///////////
        return services;
    }
}