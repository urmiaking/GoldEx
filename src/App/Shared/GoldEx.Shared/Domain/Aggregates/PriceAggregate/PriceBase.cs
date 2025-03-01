using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.PriceAggregate;

public readonly record struct PriceId(Guid Value);
public abstract class PriceBase<TPriceHistory> : EntityBase<PriceId> where TPriceHistory : PriceHistoryBase
{
    protected PriceBase(string title, MarketType marketType, string? iconFile = null) : base(new PriceId(Guid.NewGuid()))
    {
        Title = title;
        MarketType = marketType;
        IconFile = iconFile;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected PriceBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public string? IconFile { get; private set; }
    public MarketType MarketType { get; private set; }
    public ICollection<TPriceHistory>? PriceHistories { get; private set; }
}