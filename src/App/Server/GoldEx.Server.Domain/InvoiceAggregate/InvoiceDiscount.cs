using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceDiscount : EntityBase
{
    public static InvoiceDiscount Create(decimal amount, PriceUnitId discountUnitId, string? description = null)
    {
        return new InvoiceDiscount
        {
            Amount = amount,
            DiscountUnitId = discountUnitId,
            Description = description
        };
    }

    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public PriceUnitId DiscountUnitId { get; private set; }
    public PriceUnit? DiscountUnit { get; private set; }
}