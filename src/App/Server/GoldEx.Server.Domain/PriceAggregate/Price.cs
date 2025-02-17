using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Sdk.Server.Infrastructure.DTOs.Enums;

namespace GoldEx.Server.Domain.PriceAggregate;

public readonly record struct PriceId(Guid Value);

public class Price : EntityBase<PriceId>
{
    private readonly List<PriceHistory> _priceHistories = [];

    public Price(string title, PriceType priceType) : base(new PriceId(Guid.NewGuid()))
    {
        Title = title;
        PriceType = priceType;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Price() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string Title { get; private set; }
    public PriceType PriceType { get; private set; }
    public IReadOnlyList<PriceHistory> PriceHistories => _priceHistories;

    public void AddPriceHistory(PriceHistory priceHistory)
    {
        _priceHistories.Add(priceHistory);
    }

    public PriceHistory? GetLatestPriceHistory()
    {
        return _priceHistories.MaxBy(x => x.Id);
    }

    public static Price CreateFromSnapshot(PriceId id, string title, PriceType priceType, PriceHistory latestHistory)
    {
        var price = new Price
        {
            Id = id,
            Title = title,
            PriceType = priceType
        };
        price._priceHistories.Add(latestHistory);
        return price;
    }
}