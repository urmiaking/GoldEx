using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;

namespace GoldEx.Client.Offline.Domain.PriceAggregate;

public class Price(PriceId id, string title, MarketType marketType, string? iconFile = null) 
    : PriceBase<PriceHistory>(id, title, marketType, iconFile);