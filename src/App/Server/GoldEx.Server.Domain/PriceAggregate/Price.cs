using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.PriceAggregate;

public readonly record struct PriceId(Guid Value);
public class Price : EntityBase<PriceId>
{
    public static Price Create(PriceCatalog priceCatalog, PriceHistory? priceHistory = null)
    {
        return new Price
        {
            Id = new PriceId(Guid.CreateVersion7()),
            Title = priceCatalog.GetDisplayName(),
            PriceCatalog = priceCatalog,
            MarketType = priceCatalog.GetMarketType(),
            PriceHistory = priceHistory,
            IsActive = true
        };
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Price() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPinned { get; private set; }
    public MarketType MarketType { get; private set; }
    public PriceCatalog PriceCatalog { get; private set; }

    public PriceHistory? PriceHistory { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }

    public void CreatePriceHistory(PriceHistory priceHistory) => PriceHistory = priceHistory;

    public void SetPriceHistory(decimal currentValue, DateTime lastUpdate, string dailyChangeRate, string unit)
    {
        if (PriceHistory is null)
            throw new InvalidOperationException("Price history is not initialized.");

        PriceHistory.SetCurrentValue(currentValue);
        PriceHistory.SetLastUpdate(lastUpdate);
        PriceHistory.SetDailyChangeRate(dailyChangeRate);
        PriceHistory.SetUnit(unit);
    }

    public void SetTitle(string title) => Title = title;
    public void SetMarketType(MarketType marketType) => MarketType = marketType;
    public void SetStatus(bool isActive) => IsActive = isActive;
    public void SetPinned(bool isPinned) => IsPinned = isPinned;

    public void SetCatalog(PriceCatalog priceCatalog) => PriceCatalog = priceCatalog;
}