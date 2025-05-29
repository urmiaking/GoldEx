using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceItem : EntityBase
{
    public InvoiceItem(ProductId productId, decimal? tax, decimal? price, decimal? quantity)
    {
        Tax = tax;
        Price = price;
        Quantity = quantity;
        ProductId = productId;
    }

    private InvoiceItem() { }

    public decimal? Tax { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? Quantity { get; private set; }

    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }
}