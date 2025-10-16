using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceUsedProductId(Guid Value);
public class InvoiceUsedProduct : EntityBase<InvoiceUsedProductId>
{
    private InvoiceUsedProduct(InvoiceUsedProductId id,
        string description,
        decimal weight,
        decimal gramPrice,
        decimal? extraCostsAmount,
        decimal fineness,
        decimal finenessDeductionRate,
        int quantity,
        bool isBroken,
        ProductType productType,
        GoldUnitType unitType,
        ProductId? productId,
        Invoice invoice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(finenessDeductionRate, -250, nameof(finenessDeductionRate));
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0, nameof(weight));
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPrice, 0, nameof(gramPrice));
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 0, nameof(quantity));

        var itemAmount = CalculatorHelper.UsedProduct.Calculate(weight, fineness, finenessDeductionRate, gramPrice, quantity, invoice.ExchangeRate);

        Id = id;
        Description = description;
        FinenessDeductionRate = finenessDeductionRate;
        Weight = weight;
        GramPrice = gramPrice;
        Quantity = quantity;
        ProductId = productId;
        Invoice = invoice;
        ProductType = productType;
        UnitType = unitType;
        ItemAmount = itemAmount;
        ItemFinalAmount = itemAmount + (extraCostsAmount ?? 0);
        ExtraCostsAmount = extraCostsAmount;
        IsBroken = isBroken;
    }

    internal static InvoiceUsedProduct Create(InvoiceUsedProductId? id,
        string description,
        decimal weight,
        decimal gramPrice,
        decimal? extraCostsAmount,
        decimal fineness,
        decimal finenessDeductionRate,
        int quantity,
        bool isBroken,
        ProductType productType,
        GoldUnitType unitType,
        ProductId? productId,
        Invoice invoice)
    {
        return new InvoiceUsedProduct(id ?? new InvoiceUsedProductId(Guid.NewGuid()),
            description,
            weight,
            gramPrice,
            extraCostsAmount,
            fineness,
            finenessDeductionRate,
            quantity,
            isBroken,
            productType,
            unitType,
            productId,
            invoice);
    }

    public string Description { get; private set; }
    public decimal FinenessDeductionRate { get; private set; }
    public decimal Weight { get; private set; } // TODO: change to total weight
    public decimal GramPrice { get; private set; }
    public int Quantity { get; private set; }
    public bool IsBroken { get; private set; }
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