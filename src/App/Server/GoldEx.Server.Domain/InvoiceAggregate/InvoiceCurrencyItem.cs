using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceCurrencyItem : InvoiceItemBase
{
    public PriceUnitId CurrencyId { get; private set; }
}