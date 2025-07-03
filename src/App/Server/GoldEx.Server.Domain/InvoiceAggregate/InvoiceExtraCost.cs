using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceExtraCost : EntityBase
{
    public static InvoiceExtraCost Create(decimal amount, decimal? exchangeRate, PriceUnitId priceUnitId, string? description = null)
    {
        return new InvoiceExtraCost
        {
            Amount = amount,
            ExchangeRate = exchangeRate,
            Description = description,
            PriceUnitId = priceUnitId
        };
    }

    public decimal Amount { get; private set; }
    public decimal? ExchangeRate { get; private set; }
    public string? Description { get; private set; }

    public PriceUnitId PriceUnitId { get; private set; }
    public PriceUnit? PriceUnit { get; private set; }
}