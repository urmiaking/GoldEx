using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceItemAggregate;

public readonly record struct InvoiceItemId(Guid Value);
public class InvoiceItem : EntityBase< InvoiceItemId>
{
    public static InvoiceItem Create(
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        int quantity,
        ProductId productId,
        PriceUnitId priceUnitId,
        decimal? exchangeRate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gramPrice, 0, nameof(gramPrice));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0, nameof(quantity));
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));

        if (exchangeRate is < 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero.");

        return new InvoiceItem
        {
            Id = new InvoiceItemId(Guid.NewGuid()),
            GramPrice = gramPrice,
            ProfitPercent = profitPercent,
            TaxPercent = taxPercent,
            Quantity = quantity,
            ProductId = productId,
            PriceUnitId = priceUnitId,
            ExchangeRate = exchangeRate
        };
    }

    private InvoiceItem() { }

    #region Properties

    public decimal ProfitPercent { get; private set; }
    public decimal TaxPercent { get; private set; }

    public decimal GramPrice { get; private set; }
    public decimal? ExchangeRate { get; private set; }

    public int Quantity { get; private set; }

    public PriceUnitId PriceUnitId { get; set; }
    public PriceUnit? PriceUnit { get; set; }

    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    #endregion

    #region Calculations

    public decimal ItemRawAmount =>
        Product is null
            ? 0
            : CalculatorHelper.CalculateRawPrice(Product.Weight, GramPrice, Product.CaratType, Product.ProductType);

    public decimal ItemWageAmount =>
        Product is null
            ? 0
            : CalculatorHelper.CalculateWage(ItemRawAmount, Product.Weight, Product.Wage, Product.WageType, ExchangeRate);

    public decimal ItemProfitAmount =>
        Product is null
            ? 0
            : CalculatorHelper.CalculateProfit(ItemRawAmount, ItemWageAmount, Product.ProductType, ProfitPercent);

    public decimal ItemTaxAmount =>
        Product is null
            ? 0
            : CalculatorHelper.CalculateTax(ItemWageAmount, ItemProfitAmount, TaxPercent, Product.ProductType);

    public decimal ItemFinalAmount =>
        Product is null
            ? 0
            : CalculatorHelper.CalculateFinalPrice(
                ItemRawAmount,
                ItemWageAmount,
                ItemProfitAmount,
                ItemTaxAmount,
                null,
                Product.ProductType);

    public decimal TotalAmount => ItemFinalAmount * Quantity;

    #endregion
}