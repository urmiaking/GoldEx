using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Transactions;

public record CreateTransactionRequest(
    Guid Id,
    long Number,
    string Description,
    DateTime DateTime,
    decimal? Credit,
    UnitType? CreditUnit,
    decimal? CreditRate,
    decimal? Debit,
    UnitType? DebitUnit,
    decimal? DebitRate,
    Guid CustomerId
    );