using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceProductItem : EntityBase
{
    public static InvoiceProductItem Create(
        decimal gramPrice,
        decimal profitPercent,
        decimal taxPercent,
        ProductId productId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(gramPrice, 0, nameof(gramPrice));

        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        ArgumentOutOfRangeException.ThrowIfLessThan(taxPercent, 0, nameof(taxPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxPercent, 100, nameof(taxPercent));

        return new InvoiceProductItem
        {
            GramPrice = gramPrice,
            ProfitPercent = profitPercent,
            TaxPercent = taxPercent,
            ProductId = productId,
            // Stored calculated values are initialized to 0
            ItemRawAmount = 0,
            ItemWageAmount = 0,
            ItemProfitAmount = 0,
            ItemTaxAmount = 0,
            ItemFinalAmount = 0
        };
    }

#pragma warning disable CS8618 
    private InvoiceProductItem() { }
#pragma warning restore CS8618

    #region Properties

    public decimal ProfitPercent { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal GramPrice { get; private set; }

    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; }

    #endregion

    #region Calculations

    public decimal ItemRawAmount { get; private set; }
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
    public void RecalculateAmounts(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id != ProductId)
            throw new ArgumentException("The provided product does not match the item's ProductId.", nameof(product));

        ItemRawAmount = CalculatorHelper.Product.CalculateRawPrice(product.Weight, GramPrice, product.CaratType, product.ProductType);
        ItemWageAmount = CalculatorHelper.Product.CalculateWage(ItemRawAmount, product.Weight, product.Wage, product.WageType, Invoice.ExchangeRate);
        ItemProfitAmount = CalculatorHelper.Product.CalculateProfit(ItemRawAmount, ItemWageAmount, product.ProductType, ProfitPercent);
        ItemTaxAmount = CalculatorHelper.Product.CalculateTax(ItemWageAmount, ItemProfitAmount, TaxPercent, product.ProductType);

        ItemFinalAmount = CalculatorHelper.Product.CalculateFinalPrice(
            ItemRawAmount,
            ItemWageAmount,
            ItemProfitAmount,
            ItemTaxAmount,
            null,
            product.ProductType);
    }

    #endregion
}