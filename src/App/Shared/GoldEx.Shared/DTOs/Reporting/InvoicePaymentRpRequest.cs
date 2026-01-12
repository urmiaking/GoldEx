using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record InvoicePaymentRpRequest(long InvoiceNumber, InvoiceType InvoiceType);