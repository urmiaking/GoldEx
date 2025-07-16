using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.DTOs.Transactions;

public record CreateTransactionRequest(
    long Number,
    string Description,
    DateTime DateTime,
    Guid PriceUnitId,
    decimal? Credit,
    Guid? CreditPriceUnitId,
    decimal? CreditRate,
    decimal? Debit,
    Guid? DebitPriceUnitId,
    decimal? DebitRate,
    CustomerRequestDto Customer);