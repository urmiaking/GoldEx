using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record GetPaymentVoucherListResponse(
    Guid Id,
    long VoucherNumber,
    string Description,
    DateOnly PaymentDate,
    decimal Amount,
    string PriceUnit,
    VoucherStatus VoucherStatus,
    PaymentVoucherType VoucherType,
    string? SupplierName,
    string? SupplierPhoneNumber,
    FinancialAccountType FinancialAccountType,
    Guid? InvoiceId);