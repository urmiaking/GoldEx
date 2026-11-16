using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public readonly record struct InvoiceCoinItemId(Guid Value);
public class InvoiceCoinItem : EntityBase<InvoiceCoinItemId>
{
    private InvoiceCoinItem(InvoiceCoinItemId id, CoinInstanceId coinInstanceId, decimal unitPrice, int quantity, decimal profitPercent, bool isInstant)
    {
        Id = id;
        CoinInstanceId = coinInstanceId;
        IsInstant = isInstant;
        RecalculateAmounts(unitPrice, quantity, profitPercent);
    }

    internal static InvoiceCoinItem Create(InvoiceCoinItemId? id, CoinInstanceId coinInstanceId, decimal unitPrice, int quantity, decimal profitPercent, bool isInstant)
    {
        return new InvoiceCoinItem(id ?? new InvoiceCoinItemId(Guid.CreateVersion7()), coinInstanceId, unitPrice, quantity, profitPercent, isInstant);
    }

    private InvoiceCoinItem() { }

    public CoinInstanceId CoinInstanceId { get; private set; }
    public CoinInstance? CoinInstance { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal ProfitPercent { get; private set; }

    public bool IsInstant { get; private set; }

    public decimal ItemRawAmount { get; private set; }
    public decimal ItemProfitAmount { get; private set; }
    public decimal ItemFinalAmount { get; private set; }

    public void Update(CoinInstanceId coinInstanceId, decimal unitPrice, int quantity, decimal profitPercent)
    {
        RecalculateAmounts(unitPrice, quantity, profitPercent);
        CoinInstanceId = coinInstanceId;
    }

    private void RecalculateAmounts(decimal unitPrice, int quantity, decimal profitPercent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(unitPrice, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(profitPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(profitPercent, 100);

        UnitPrice = unitPrice;
        Quantity = quantity;
        ProfitPercent = profitPercent;

        ItemRawAmount = unitPrice;
        ItemProfitAmount = CalculatorHelper.Coin.CalculateProfit(unitPrice, profitPercent, quantity);
        ItemFinalAmount = (unitPrice * quantity) + ItemProfitAmount;
    }
}
