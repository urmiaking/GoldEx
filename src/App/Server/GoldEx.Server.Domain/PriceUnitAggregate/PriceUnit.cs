using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.PriceUnitAggregate;

public readonly record struct PriceUnitId(Guid Value);
public class PriceUnit : EntityBase<PriceUnitId>
{
    public static PriceUnit Create(
        string title,
        UnitType? unitType,
        bool isDefault = false,
        PriceId? priceId = null)
    {
        return new PriceUnit
        {
            Id = new PriceUnitId(Guid.NewGuid()),
            Title = title,
            PriceId = priceId,
            UnitType = unitType,
            IsActive = true,
            IsDefault = isDefault
        };
    }

#pragma warning disable CS8618
    private PriceUnit() { }
#pragma warning restore CS8618

    public string Title { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public UnitType? UnitType { get; private set; }

    public PriceId? PriceId { get; private set; }
    public Price? Price { get; private set; }

    public void SetTitle(string title) => Title = title;
    public void SetPriceId(PriceId? priceId) => PriceId = priceId;
    public void SetStatus(bool isActive) => IsActive = isActive;
    public void SetDefault(bool isDefault) => IsDefault = isDefault;

    public override bool Equals(object? obj)
    {
        if (obj is not PriceUnit other)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public bool IsGoldBased => UnitType is Shared.Enums.UnitType.Gold18K or Shared.Enums.UnitType.Mesghal;
}