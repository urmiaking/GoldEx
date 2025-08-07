using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record GetPaymentVoucherResponse(
    Guid Id,
    long VoucherNumber,
    decimal Amount,
    decimal? ExchangeRate,
    string Description,
    DateTime PaymentDate,
    PaymentVoucherType VoucherType,
    GetCustomerResponse Customer,
    GetPriceUnitTitleResponse PriceUnit,
    GetFinancialAccountTitleResponse SourceFinancialAccount,
    GetFinancialAccountTitleResponse DestinationFinancialAccount);