using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record CustomerTransactionRpResponse(
    DateTime PostingDate,
    string Description,
    TransactionType TransactionType,
    LedgerAccountRole Role,
    decimal Amount,
    decimal RunningBalance,
    string PriceUnitTitle,
    decimal? ExchangeRate,
    decimal BaseCurrencyAmount,
    Guid? InvoiceId,
    Guid? InvoicePaymentId,
    Guid? PaymentVoucherId);