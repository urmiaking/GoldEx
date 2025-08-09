using GoldEx.Server.Domain.CoinAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceCoinItem : InvoiceItemBase
{
    public CoinId CoinId { get; private set; }
}