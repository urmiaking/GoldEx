using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Services;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Services.Price;
using GoldEx.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services.PriceProviders;

[ScopedService]
public class PriceProviderRegistry(IServiceProvider serviceProvider, ILogger<PriceProviderRegistry> logger)
{
    private static readonly Dictionary<PriceProviderType, Type> ProviderTypeMap = new()
    {
        { PriceProviderType.Signal, typeof(GenericBatchPriceProvider<SignalPriceFetcher>) },
        { PriceProviderType.TalaIr, typeof(GenericBatchPriceProvider<TalaIrPriceFetcher>) },
        { PriceProviderType.BrsApi, typeof(GenericBatchPriceProvider<BrsApiPriceFetcher>) },
        { PriceProviderType.Tjgu, typeof(GenericBatchPriceProvider<TjguPriceFetcher>) }
    };

    public IBatchPriceProvider? Resolve(PriceProviderType providerType)
    {
        if (providerType == PriceProviderType.Manual)
            return null;

        if (!ProviderTypeMap.TryGetValue(providerType, out var providerConcreteType))
        {
            logger.LogWarning("ProviderType {ProviderType} not mapped to any batch provider type.", providerType);
            return null;
        }

        // گرفتن سرویس ثبت‌شده در DI Container
        var service = serviceProvider.GetService(providerConcreteType);

        var instance = service as IBatchPriceProvider;

        if (instance is null)
        {
            logger.LogWarning("No DI registration found for provider concrete type {Type}.", providerConcreteType.FullName);
        }

        return instance;
    }
}