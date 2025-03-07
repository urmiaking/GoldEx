using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;

namespace GoldEx.Server.Domain.PriceAggregate;

public class Price(string title, MarketType marketType, string? iconFile = null)
    : PriceBase<PriceHistory>(title, marketType, iconFile);