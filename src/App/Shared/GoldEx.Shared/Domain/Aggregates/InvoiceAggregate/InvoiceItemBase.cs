using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Shared.Domain.Aggregates.InvoiceAggregate;

public class InvoiceItemBase<TProduct, TCategory, TGemStone> : EntityBase
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{
    public double? Tax { get; private set; }
    public double? Price { get; private set; }
    public double? Quantity { get; private set; }

    public ProductId ProductId { get; private set; }
    public TProduct? Product { get; private set; }
}