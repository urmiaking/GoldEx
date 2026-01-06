using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public sealed class LedgerAccountStatementModel
{
    public DateTime PostingDate { get; set; }
    public string Description { get; set; } = default!;
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
    public string PriceUnitTitle { get; set; } = default!;
    public decimal? ExchangeRate { get; set; }
    public decimal BaseCurrencyAmount { get; set; }

    public Guid? InvoiceId { get; set; }
    public Guid? PaymentVoucherId { get; set; }
    public Guid? InvoicePaymentId { get; set; }
    public Guid? InventoryEntryId { get; set; }
    public Guid? InventoryExitId { get; set; }
    public Guid? InventoryStockId { get; set; }
    public Guid? MeltingBatchId { get; set; }
}