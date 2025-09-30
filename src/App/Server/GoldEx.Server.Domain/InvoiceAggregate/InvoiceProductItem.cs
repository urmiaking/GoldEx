using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceProductItemId(Guid Value);
public class InvoiceProductItem : EntityBase<InvoiceProductItemId>
{
    private InvoiceProductItem(InvoiceProductItemId id,
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        ProductId productId,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct,
        decimal? saleWage,
        WageType? saleWageType,
        PriceUnitId? saleWagePriceUnitId,
        decimal? saleWagePriceUnitExchangeRate,
        decimal? stonePriceUnitExchangeRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gramPrice, 0, nameof(gramPrice));
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0, nameof(quantity));


        Id = id;
        GramPrice = gramPrice;
        ProfitPercent = profitPercent;
        TaxPercent = taxPercent;
        ProductId = productId;
        CostPrice = costPrice;
        CostPriceExchangeRate = costPriceExchangeRate;
        CostPriceUnitId = costPriceUnitId;
        IsInstantProduct = isInstantProduct;
        Quantity = quantity;
        SaleWage = saleWage;
        SaleWageType = saleWageType;
        SaleWagePriceUnitId = saleWagePriceUnitId;
        SaleWagePriceUnitExchangeRate = saleWagePriceUnitExchangeRate;
        StonePriceUnitExchangeRate = stonePriceUnitExchangeRate;

        // Stored calculated values are initialized to 0
        ItemRawAmount = 0;
        ItemWageAmount = 0;
        ItemProfitAmount = 0;
        ItemTaxAmount = 0;
        ItemFinalAmount = 0;
        ItemStoneAmount = 0;
    }

    internal static InvoiceProductItem CreatePurchaseItem(InvoiceProductItemId? id,
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        ProductId productId,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        decimal? stonePriceUnitExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct)
    {
        return new InvoiceProductItem(id ?? new InvoiceProductItemId(Guid.NewGuid()),
            gramPrice,
            profitPercent,
            taxPercent,
            quantity,
            productId,
            costPrice,
            costPriceExchangeRate,
            costPriceUnitId,
            isInstantProduct,
            null,
            null,
            null,
            null,
            stonePriceUnitExchangeRate);
    }

    internal static InvoiceProductItem CreateSaleItem(InvoiceProductItemId? id,
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        ProductId productId,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct,
        decimal saleWage,
        WageType? saleWageType,
        PriceUnitId? saleWagePriceUnitId,
        decimal? saleWagePriceUnitExchangeRate,
        decimal? stonePriceUnitExchangeRate)
    {
        return new InvoiceProductItem(id ?? new InvoiceProductItemId(Guid.NewGuid()),
            gramPrice,
            profitPercent,
            taxPercent,
            quantity,
            productId,
            costPrice,
            costPriceExchangeRate,
            costPriceUnitId,
            isInstantProduct,
            saleWage,
            saleWageType,
            saleWagePriceUnitId,
            saleWagePriceUnitExchangeRate,
            stonePriceUnitExchangeRate);
    }

#pragma warning disable CS8618 
    private InvoiceProductItem() { }
#pragma warning restore CS8618

    #region Properties

    public decimal ProfitPercent { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal GramPrice { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal? CostPriceExchangeRate { get; private set; }
    public bool IsInstantProduct { get; private set; }
    public int Quantity { get; private set; }

    public PriceUnitId? CostPriceUnitId { get; private set; }
    public PriceUnit? CostPriceUnit { get; private set; }

    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal? SaleWage { get; private set; }
    public WageType? SaleWageType { get; private set; }
    public PriceUnitId? SaleWagePriceUnitId { get; private set; }
    public PriceUnit? SaleWagePriceUnit { get; private set; }
    public decimal? SaleWagePriceUnitExchangeRate { get; set; }

    public decimal? StonePriceUnitExchangeRate { get; private set; }

    #endregion

    #region Calculations

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemStoneAmount { get; private set; }
    public decimal ItemWageAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemTaxAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }

    /// <summary>
    /// Calculates all financial amounts for the invoice item and updates its state.
    /// This method should be called after creation or after any property that affects
    /// totals has been changed.
    /// </summary>
    /// <param name="product">The fully loaded Product entity, required for calculation.</param>
    /// <param name="invoiceType">invoice type</param>
    public InvoiceProductItem RecalculateAmounts(Product product, InvoiceType invoiceType)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id != ProductId)
            throw new ArgumentException("The provided product does not match the item's ProductId.", nameof(product));

        if (invoiceType is InvoiceType.Purchase)
        {
            ItemRawAmount = (CostPrice ?? 0) * Quantity;
            ItemWageAmount = 0;
            ItemProfitAmount = 0;
            ItemTaxAmount = 0;
            ItemStoneAmount = 0;
            ItemFinalAmount = ItemRawAmount;
        }
        else
        {
            ItemStoneAmount = product.GemStones.Sum(x => x.Cost * (StonePriceUnitExchangeRate ?? 1)) * Quantity;

            ItemRawAmount = CalculatorHelper.Product.CalculateRawPrice(product.Weight,
                                                                       GramPrice,
                                                                       product.Fineness,
                                                                       Quantity,
                                                                       product.ProductType);
            ItemWageAmount = SaleWageType is not null && SaleWage.HasValue
                    ? CalculatorHelper.Product.CalculateWage(ItemRawAmount,
                                                             product.Weight,
                                                             SaleWage.Value,
                                                             SaleWageType.Value,
                                                             SaleWagePriceUnitExchangeRate ?? Invoice.ExchangeRate)
                    : CalculatorHelper.Product.CalculateWage(ItemRawAmount,
                                                             product.Weight,
                                                             product.Wage,
                                                             product.WageType,
                                                             Invoice.ExchangeRate);
            ItemProfitAmount = CalculatorHelper.Product.CalculateProfit(ItemRawAmount,
                                                                        ItemWageAmount,
                                                                        product.ProductType,
                                                                        ProfitPercent);
            ItemTaxAmount = CalculatorHelper.Product.CalculateTax(ItemWageAmount,
                                                                  ItemProfitAmount,
                                                                  TaxPercent,
                                                                  product.ProductType,
                                                                  ItemStoneAmount);

            ItemFinalAmount = CalculatorHelper.Product.CalculateFinalPrice(ItemRawAmount,
                                                                           ItemWageAmount,
                                                                           ItemProfitAmount,
                                                                           ItemTaxAmount,
                                                                           null,
                                                                           product.ProductType);
        }

        return this;
    }

    /// <summary>
    /// Sets the Product for this invoice item and recalculates all amounts. This method should be called before calling RecalculateAmounts.
    /// </summary>
    /// <param name="invoice"></param>
    /// <returns></returns>
    public InvoiceProductItem SetInvoice(Invoice invoice)
    {
        Invoice = invoice;
        return this;
    }

    #endregion

    public void UpdatePurchaseItem(decimal gramPrice, 
        int quantity,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        PriceUnitId? costPriceUnitId)
    {
        GramPrice = gramPrice;
        Quantity = quantity;
        CostPrice = costPrice;
        CostPriceExchangeRate = costPriceExchangeRate;
        CostPriceUnitId = costPriceUnitId;
    }

    public void UpdateSaleItem(decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        decimal? costPrice,
        decimal? costPriceExchangeRate,
        PriceUnitId? costPriceUnitId,
        bool isInstantProduct,
        decimal saleWage,
        WageType? saleWageType,
        PriceUnitId? saleWagePriceUnitId,
        decimal? saleWagePriceUnitExchangeRate,
        decimal? stonePriceUnitExchangeRate)
    {
        GramPrice = gramPrice;
        ProfitPercent = profitPercent;
        TaxPercent = taxPercent;
        Quantity = quantity;
        CostPrice = costPrice;
        CostPriceExchangeRate = costPriceExchangeRate;
        CostPriceUnitId = costPriceUnitId;
        IsInstantProduct = isInstantProduct;
        SaleWage = saleWage;
        SaleWageType = saleWageType;
        SaleWagePriceUnitId = saleWagePriceUnitId;
        SaleWagePriceUnitExchangeRate = saleWagePriceUnitExchangeRate;
        StonePriceUnitExchangeRate = stonePriceUnitExchangeRate;
    }
}