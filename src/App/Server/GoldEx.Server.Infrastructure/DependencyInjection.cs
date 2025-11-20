using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Models.Ai;
using GoldEx.Server.Infrastructure.Services;
using GoldEx.Server.Infrastructure.Services.Price;
using GoldEx.Shared;
using GoldEx.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // --- TalaIr ---
        services.AddHttpClient("TalaIrApi");
        services.AddScoped<TalaIrPriceFetcher>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("TalaIrApi");
            return new TalaIrPriceFetcher(httpClient);
        });
        services.AddScoped<IPriceFetcher, TalaIrPriceFetcher>(sp => sp.GetRequiredService<TalaIrPriceFetcher>());

        // --- Signal ---
        services.AddHttpClient("SignalApi");
        services.AddScoped<SignalPriceFetcher>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("SignalApi");
            return new SignalPriceFetcher(httpClient, Utilities.GetJsonOptions());
        });
        services.AddScoped<IPriceFetcher, SignalPriceFetcher>(sp => sp.GetRequiredService<SignalPriceFetcher>());

        // --- Brs ---
        services.AddHttpClient("BrsApi");
        services.AddScoped<BrsApiPriceFetcher>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("BrsApi");
            var options = sp.GetRequiredService<IOptions<PriceProviderSetting>>();
            return new BrsApiPriceFetcher(httpClient, Utilities.GetJsonOptions(), options);
        });
        services.AddScoped<IPriceFetcher, BrsApiPriceFetcher>(sp => sp.GetRequiredService<BrsApiPriceFetcher>());

        // --- Tjgu ---
        services.AddHttpClient("TjguApi");
        services.AddScoped<TjguPriceFetcher>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("TjguApi");
            return new TjguPriceFetcher(httpClient, Utilities.GetJsonOptions());
        });
        services.AddScoped<IPriceFetcher, TjguPriceFetcher>(sp => sp.GetRequiredService<TjguPriceFetcher>());

        services.DiscoverServices();

        // --- Generic Batch Providers ---
        services.AddScoped(typeof(GenericBatchPriceProvider<>));

        services.AddPredictionEnginePool<CategoryModelInput, CategoryModelOutput>().FromFile("wwwroot/models/ProductCategory.mlnet", true);

        return services;
    }
}