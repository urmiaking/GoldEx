using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceUsedProduct : EntityBase
{
    internal static InvoiceUsedProduct Create(string description,
        decimal weight,
        decimal gramPrice,
        decimal? extraCostsAmount,
        decimal fineness,
        bool isSellable,
        ProductId? productId,
        Invoice invoice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fineness, 0, nameof(fineness));
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0, nameof(weight));
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPrice, 0, nameof(gramPrice));

        if (!isSellable && productId.HasValue)
            throw new ArgumentException("If the trade-in is not sellable, product ID should not be provided.",
                nameof(isSellable));

        var itemAmount = CalculatorHelper.UsedProduct.Calculate(weight, fineness, gramPrice, invoice.ExchangeRate);

        return new InvoiceUsedProduct
        {
            Description = description,
            Fineness = fineness,
            Weight = weight,
            GramPrice = gramPrice,
            IsSellable = isSellable,
            ProductId = productId,
            Invoice = invoice,
            ItemAmount = itemAmount,
            ExtraCostsAmount = extraCostsAmount,
            ItemFinalAmount = itemAmount + (extraCostsAmount ?? 0)
        };
    }

    public string Description { get; private set; }
    public decimal Fineness { get; private set; }
    public decimal Weight { get; private set; }
    public decimal GramPrice { get; private set; }
    public bool IsSellable { get; private set; }

    public ProductId? ProductId { get; private set; }
    public Product? Product { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal ItemAmount { get; private set; }
    public decimal? ExtraCostsAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }

#pragma warning disable CS8618 
    private InvoiceUsedProduct() { }
#pragma warning restore CS8618
}