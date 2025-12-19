using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceCoinItemId(Guid Value);
public class InvoiceCoinItem : EntityBase<InvoiceCoinItemId>
{
    private InvoiceCoinItem(InvoiceCoinItemId id, CoinInstanceId coinInstanceId, decimal unitPrice, int quantity, decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(unitPrice, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100);

        var itemProfitAmount = CalculatorHelper.Coin.CalculateProfit(unitPrice, profitPercent, quantity);
        var itemFinalAmount = (unitPrice * quantity) + itemProfitAmount;

        Id = id;
        CoinInstanceId = coinInstanceId;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ProfitPercent = profitPercent;

        ItemRawAmount = unitPrice;
        ItemProfitAmount = itemProfitAmount;
        ItemFinalAmount = itemFinalAmount;
    }

    internal static InvoiceCoinItem Create(InvoiceCoinItemId? id, CoinInstanceId coinInstanceId, decimal unitPrice, int quantity, decimal profitPercent)
    {
        return new InvoiceCoinItem(id ?? new InvoiceCoinItemId(Guid.NewGuid()), coinInstanceId, unitPrice, quantity, profitPercent);
    }

    private InvoiceCoinItem() { }

    public CoinInstanceId CoinInstanceId { get; private set; }
    public CoinInstance? CoinInstance { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal ProfitPercent { get; private set; }

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }
}