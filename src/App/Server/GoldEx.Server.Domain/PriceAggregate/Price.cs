using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceHistoryAggregate;

namespace GoldEx.Server.Domain.PriceAggregate;

public readonly record struct PriceId(Guid Value);
public class Price : EntityBase<PriceId>
{
    public Price(string title, MarketType marketType, string? iconUrl = null) : base(new PriceId(Guid.NewGuid()))
    {
        Title = title;
        MarketType = marketType;
        IconUrl = iconUrl;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Price() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public string? IconUrl { get; private set; }
    public MarketType MarketType { get; private set; }
    public IReadOnlyList<PriceHistory>? PriceHistories { get; private set; } 
}