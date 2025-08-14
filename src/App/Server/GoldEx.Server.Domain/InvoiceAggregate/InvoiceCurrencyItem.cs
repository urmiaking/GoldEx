using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceCurrencyItemId(Guid Value);
public class InvoiceCurrencyItem : EntityBase<InvoiceCurrencyItemId>
{
    private InvoiceCurrencyItem(InvoiceCurrencyItemId id, PriceUnitId currencyId, decimal unitPrice, decimal amount,
        decimal taxPercent, decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(unitPrice, 0, nameof(unitPrice));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0, nameof(amount));
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        Id = id;
        CurrencyId = currencyId;
        UnitPrice = unitPrice;
        Amount = amount;
        ProfitPercent = profitPercent;
        TaxPercent = taxPercent;
        ItemRawAmount = unitPrice;
        ItemProfitAmount = CalculatorHelper.Currency.CalculateProfit(unitPrice, amount, profitPercent);
        ItemTaxAmount = CalculatorHelper.Currency.CalculateTax(unitPrice, amount, taxPercent);
        ItemFinalAmount = unitPrice + ItemProfitAmount + ItemTaxAmount;
        TotalAmount = ItemFinalAmount * amount;
    }

    public static InvoiceCurrencyItem Create(PriceUnitId currencyId,
        decimal unitPrice,
        decimal amount,
        decimal taxPercent,
        decimal profitPercent)
    {
        return new InvoiceCurrencyItem(new InvoiceCurrencyItemId(Guid.NewGuid()), currencyId, unitPrice, amount, taxPercent, profitPercent);
    }

    public static InvoiceCurrencyItem Create(InvoiceCurrencyItemId id,
        PriceUnitId currencyId,
        decimal unitPrice,
        decimal amount,
        decimal taxPercent,
        decimal profitPercent)
    {
        return new InvoiceCurrencyItem(id, currencyId, unitPrice, amount, taxPercent, profitPercent);
    }

    public PriceUnitId CurrencyId { get; private set; }
    public PriceUnit? Currency { get; private set; }

    public decimal UnitPrice { get; private set; }
    public decimal Amount { get; private set; }
    public decimal ProfitPercent { get; private set; }
    public decimal TaxPercent { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

#pragma warning disable CS8618 
    private InvoiceCurrencyItem() { }
#pragma warning restore CS8618 

    #region Calculations

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal ItemTaxAmount { get; private set; }

    #endregion
}
