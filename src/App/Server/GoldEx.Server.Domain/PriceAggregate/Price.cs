using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.PriceAggregate;

public readonly record struct PriceId(Guid Value);
public class Price : EntityBase<PriceId>
{
    public static Price Create(
        PriceId id,
        string title,
        MarketType marketType,
        UnitType? unitType = null)
    {
        return new Price
        {
            Id = id,
            Title = title,
            MarketType = marketType,
            UnitType = unitType,
            IsActive = true
        };
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Price() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public bool IsActive { get; private set; }
    public MarketType MarketType { get; private set; }
    public UnitType? UnitType { get; private set; }

    public PriceHistory? PriceHistory { get; private set; }

    public void SetPriceHistory(PriceHistory priceHistory) => PriceHistory = priceHistory;
    public void SetTitle(string title) => Title = title;
    public void SetMarketType(MarketType marketType) => MarketType = marketType;
    public void SetUnitType(UnitType? unitType) => UnitType = unitType;

    public void SetActive()
    {
        if (IsActive)
            throw new InvalidOperationException("Price is already active.");
        IsActive = true;
    }

    public void SetInactive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Price is already inactive.");
        IsActive = false;
    }
}