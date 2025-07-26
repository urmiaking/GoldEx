using GoldEx.Shared.DTOs.FinancialAccounts;

namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record GetPaymentVoucherResponse(
    Guid Id,
    long VoucherNumber,
    decimal Amount,
    decimal? ExchangeRate,
    string Description,
    DateOnly PaymentDate,
    GetFinancialAccountResponse SourceFinancialAccount,
    GetFinancialAccountResponse DestinationFinancialAccount);