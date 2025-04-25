using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Offline.Domain.PriceAggregate;

public class Price : PriceBase<PriceHistory>
{
    public Price(PriceId id, string title, MarketType marketType, UnitType? unitType, string? iconFile = null) : base(id, title, marketType, unitType, iconFile)
    {
    }

    private Price() { }
}