using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record LedgerAccountStatementRpResponse(
    DateTime PostingDate,
    string Description,
    TransactionType TransactionType,
    decimal Amount,
    string PriceUnitTitle,
    decimal? ExchangeRate,
    decimal BaseCurrencyAmount,
    Guid? InvoiceId,
    Guid? PaymentVoucherId,
    Guid? InvoicePaymentId,
    Guid? InventoryEntryId,
    Guid? InventoryExitId,
    Guid? InventoryStockId,
    Guid? MeltingBatchId);