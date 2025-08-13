using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceCoinItemId(Guid Value);
public class InvoiceCoinItem : EntityBase<InvoiceCoinItemId>
{
    public static InvoiceCoinItem Create(CoinId coinId,
        decimal unitPrice,
        int quantity,
        decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(unitPrice, 0, nameof(unitPrice));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0, nameof(quantity));

        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0, nameof(profitPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100, nameof(profitPercent));

        return new InvoiceCoinItem
        {
            Id = new InvoiceCoinItemId(Guid.NewGuid()),
            CoinId = coinId,
            UnitPrice = unitPrice,
            Quantity = quantity,
            ProfitPercent = profitPercent,

            ItemRawAmount = unitPrice,
            ItemProfitAmount = CalculatorHelper.Coin.CalculateProfit(unitPrice, profitPercent),
            ItemFinalAmount = unitPrice + CalculatorHelper.Coin.CalculateProfit(unitPrice, profitPercent),
            TotalAmount = (unitPrice + CalculatorHelper.Coin.CalculateProfit(unitPrice, profitPercent)) * quantity
        };
    }

    public CoinId CoinId { get; private set; }
    public Coin? Coin { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal ProfitPercent { get; private set; }

    private InvoiceCoinItem() { }

    #region Calculations

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    #endregion
}