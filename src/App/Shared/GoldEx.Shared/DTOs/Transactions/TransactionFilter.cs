namespace GoldEx.Shared.DTOs.Transactions;

public record TransactionFilter(Guid? InvoiceId, DateTime? Start, DateTime? End);