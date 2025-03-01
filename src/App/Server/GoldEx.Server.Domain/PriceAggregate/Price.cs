using GoldEx.Sdk.Common.Definitions;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;

namespace GoldEx.Server.Domain.PriceAggregate;

public class Price(string title, MarketType marketType, string? iconUrl = null)
    : PriceBase<PriceHistory>(title, marketType, iconUrl)
{
}