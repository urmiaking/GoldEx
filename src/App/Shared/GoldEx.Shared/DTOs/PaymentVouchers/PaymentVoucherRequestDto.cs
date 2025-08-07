using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record PaymentVoucherRequestDto(
    Guid? Id,
    long VoucherNumber,
    decimal Amount,
    decimal? ExchangeRate,
    string Description,
    DateOnly PaymentDate,
    PaymentVoucherType VoucherType,
    Guid VoucherPriceUnitId,
    Guid SourceFinancialAccountId,
    Guid DestinationFinancialAccountId);