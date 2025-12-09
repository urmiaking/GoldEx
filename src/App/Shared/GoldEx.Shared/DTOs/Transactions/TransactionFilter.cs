namespace GoldEx.Shared.DTOs.Transactions;

public record TransactionFilter(Guid? InvoiceId, Guid? CustomerId, DateTime? Start, DateTime? End, bool ShowReversed, bool Descending);