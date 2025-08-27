using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceUsedProduct : EntityBase
{
    internal static InvoiceUsedProduct Create(string description,
        decimal weight,
        decimal gramPrice,
        decimal? extraCostsAmount,
        decimal fineness,
        int quantity,
        bool isSellable,
        ProductType productType,
        GoldUnitType unitType,
        ProductId? productId,
        Invoice invoice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fineness, 0, nameof(fineness));
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0, nameof(weight));
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPrice, 0, nameof(gramPrice));
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0, nameof(quantity));

        if (!isSellable && productId.HasValue)
            throw new ArgumentException("If the trade-in is not sellable, product ID should not be provided.",
                nameof(isSellable));

        if (productType != ProductType.Jewelry && productType != ProductType.Gold)
            throw new ArgumentException(
                "Product ID can only be provided for Jewelry or Gold product types.", nameof(productId));

        var itemAmount = CalculatorHelper.UsedProduct.Calculate(weight, fineness, gramPrice, quantity, invoice.ExchangeRate);

        return new InvoiceUsedProduct
        {
            Description = description,
            Fineness = fineness,
            Weight = weight,
            GramPrice = gramPrice,
            IsSellable = isSellable,
            Quantity = quantity,
            ProductId = productId,
            Invoice = invoice,
            ProductType = productType,
            UnitType = unitType,
            ItemAmount = itemAmount,
            ItemFinalAmount = itemAmount + (extraCostsAmount ?? 0),
            ExtraCostsAmount = extraCostsAmount
        };
    }

    public string Description { get; private set; }
    public decimal Fineness { get; private set; }
    public decimal Weight { get; private set; }
    public decimal GramPrice { get; private set; }
    public bool IsSellable { get; private set; }
    public int Quantity { get; private set; }
    public ProductType ProductType { get; private set; }
    public GoldUnitType UnitType { get; private set; }

    public ProductId? ProductId { get; private set; }
    public Product? Product { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal ItemAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }
    public decimal? ExtraCostsAmount { get; private set; }

#pragma warning disable CS8618 
    private InvoiceUsedProduct() { }
#pragma warning restore CS8618
}