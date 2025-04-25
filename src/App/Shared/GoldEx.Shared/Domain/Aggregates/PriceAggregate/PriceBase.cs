using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Domain.Aggregates.PriceAggregate;

public readonly record struct PriceId(Guid Value);
public abstract class PriceBase<TPriceHistory> : EntityBase<PriceId> where TPriceHistory : PriceHistoryBase
{
    protected PriceBase(string title, MarketType marketType, UnitType? unitType, string? iconFile = null) : base(new PriceId(Guid.NewGuid()))
    {
        Title = title;
        MarketType = marketType;
        UnitType = unitType;
        IconFile = iconFile;
    }

    protected PriceBase(PriceId id, string title, MarketType marketType, UnitType? unitType, string? iconFile = null) : base(id)
    {
        Title = title;
        MarketType = marketType;
        IconFile = iconFile;
        UnitType = unitType;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected PriceBase()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public string? IconFile { get; private set; }
    public MarketType MarketType { get; private set; }
    public UnitType? UnitType { get; private set; }

    public TPriceHistory PriceHistory { get; private set; }

    public void SetPriceHistory(TPriceHistory priceHistory) => PriceHistory = priceHistory;

    public void SetTitle(string title) => Title = title;

    public void SetIconFile(string iconFile) => IconFile = iconFile;

    public void SetMarketType(MarketType marketType) => MarketType = marketType;
}