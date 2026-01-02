namespace GoldEx.Shared.DTOs.Transactions;

public record TransactionFilter(
    Guid? InvoiceId,
    Guid? CustomerId,
    Guid? PriceUnitId,
    Guid? LedgerAccountId,
    DateTime? Start,
    DateTime? End,
    bool ShowReversed,
    bool Descending);