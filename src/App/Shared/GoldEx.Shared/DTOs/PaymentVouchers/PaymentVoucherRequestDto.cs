namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record PaymentVoucherRequestDto(
    Guid? Id,
    long VoucherNumber,
    decimal Amount,
    decimal? ExchangeRate,
    string Description,
    DateOnly PaymentDate,
    Guid VoucherPriceUnitId,
    Guid SourceFinancialAccountId,
    Guid DestinationFinancialAccountId);