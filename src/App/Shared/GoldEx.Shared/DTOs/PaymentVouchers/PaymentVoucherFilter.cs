using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.PaymentVouchers;

public record PaymentVoucherFilter(DateTime? Start, DateTime? End, PaymentVoucherType? VoucherType);