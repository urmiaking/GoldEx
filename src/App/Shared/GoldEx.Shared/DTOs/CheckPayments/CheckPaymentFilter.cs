using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.CheckPayments;

public record CheckPaymentFilter(
    CheckPaymentStatus? Status,
    DateTime? StartDueDate,
    DateTime? EndDueDate);
