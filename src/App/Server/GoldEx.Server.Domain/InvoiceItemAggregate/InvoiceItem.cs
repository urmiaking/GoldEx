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
        InvoiceId invoiceId,
        decimal? exchangeRate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gramPrice, 0, nameof(gramPrice));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0, nameof(quantity));

        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));

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
            ExchangeRate = exchangeRate,
            InvoiceId = invoiceId,
            // Stored calculated values are initialized to 0
            ItemRawAmount = 0,
            ItemWageAmount = 0,
            ItemProfitAmount = 0,
            ItemTaxAmount = 0,
            ItemFinalAmount = 0,
            TotalAmount = 0
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

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemWageAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemTaxAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Calculates all financial amounts for the invoice item and updates its state.
    /// This method should be called after creation or after any property that affects
    /// totals has been changed.
    /// </summary>
    /// <param name="product">The fully loaded Product entity, required for calculation.</param>
    public void RecalculateAmounts(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id != ProductId)
        {
            throw new ArgumentException("The provided product does not match the item's ProductId.", nameof(product));
        }

        ItemRawAmount = CalculatorHelper.CalculateRawPrice(product.Weight, GramPrice, product.CaratType, product.ProductType);
        ItemWageAmount = CalculatorHelper.CalculateWage(ItemRawAmount, product.Weight, product.Wage, product.WageType, ExchangeRate);
        ItemProfitAmount = CalculatorHelper.CalculateProfit(ItemRawAmount, ItemWageAmount, product.ProductType, ProfitPercent);
        ItemTaxAmount = CalculatorHelper.CalculateTax(ItemWageAmount, ItemProfitAmount, TaxPercent, product.ProductType);

        ItemFinalAmount = CalculatorHelper.CalculateFinalPrice(
            ItemRawAmount,
            ItemWageAmount,
            ItemProfitAmount,
            ItemTaxAmount,
            null,
            product.ProductType);

        TotalAmount = ItemFinalAmount * Quantity;
    }

    #endregion

    #region Methods

    public void SetGramPrice(decimal gramPrice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gramPrice, 0, nameof(gramPrice));
        GramPrice = gramPrice;
    }

    public void SetQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0, nameof(quantity));
        Quantity = quantity;
    }

    public void SetProfitPercent(decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));
        ProfitPercent = profitPercent;
    }

    public void SetTaxPercent(decimal taxPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));
        TaxPercent = taxPercent;
    }

    public void SetExchangeRate(decimal? exchangeRate)
    {
        if (exchangeRate is < 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero.");

        ExchangeRate = exchangeRate;
    }

    public void SetPriceUnitId(PriceUnitId priceUnitId)
    {
        PriceUnitId = priceUnitId;
    }

    #endregion
}