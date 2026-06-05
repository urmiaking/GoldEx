using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.CheckPayments;

public record GetCheckPaymentListResponse(
    Guid Id,
    Guid InvoicePaymentId,
    string? Number,
    string? SayadiCode,
    DateTime DueDate,
    CheckPaymentStatus CurrentStatus,
    DateTime LastModifiedAt,
    Guid IssuerId,
    string IssuerFullName,
    string? IssuerPhoneNumber,
    Guid IssuerFinancialAccountId,
    string IssuerFinancialAccountName,
    decimal Amount,
    string PriceUnit,
    Guid PriceUnitId,
    long InvoiceNumber,
    InvoiceType InvoiceType,
    string? ImageUrl,
    string? TargetFinancialAccountName,
    string? Description);
