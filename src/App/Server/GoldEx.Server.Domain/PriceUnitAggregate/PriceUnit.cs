using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Domain.PriceUnitAggregate;

public readonly record struct PriceUnitId(Guid Value);
public class PriceUnit : EntityBase<PriceUnitId>
{
    public static PriceUnit Create(
        string title,
        PriceId? priceId = null)
    {
        return new PriceUnit
        {
            Id = new PriceUnitId(Guid.NewGuid()),
            Title = title,
            PriceId = priceId,
            IsActive = true
        };
    }

#pragma warning disable CS8618
    private PriceUnit() { }
#pragma warning restore CS8618

    public string Title { get; private set; }
    public bool IsActive { get; private set; } = true;

    public PriceId? PriceId { get; private set; }
    public Price? Price { get; private set; }

    public void SetTitle(string title) => Title = title;
    public void SetPriceId(PriceId? priceId) => PriceId = priceId;
    public void SetStatus(bool isActive) => IsActive = isActive;

    //TODO: add a utility method to get the price exchange rate!
}