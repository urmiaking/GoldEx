using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.PriceAggregate;

public class Price : PriceBase<PriceHistory>
{
    public Price(string title, MarketType marketType, UnitType? unitType, string? iconFile = null) : base(title, marketType, unitType, iconFile)
    {
    }

    private Price() { }
}