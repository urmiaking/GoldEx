using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Shared.DTOs.Transactions;

public record GetTransactionResponse(Guid Id, 
    long Number, 
    string Description,
    DateTime DateTime,
    decimal? Credit,
    GetPriceUnitTitleResponse? CreditPriceUnit,
    decimal? CreditRate,
    decimal? Debit,
    GetPriceUnitTitleResponse? DebitPriceUnit,
    decimal? DebitRate,
    GetCustomerResponse Customer);