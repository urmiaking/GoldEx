using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Transactions;

public record CreateTransactionRequest(
    Guid Id,
    int Number,
    string Description,
    DateTime DateTime,
    double? Credit,
    UnitType? CreditUnit,
    double? CreditRate, 
    double? Debit,
    UnitType? DebitUnit,
    double? DebitRate,
    Guid CustomerId
    );