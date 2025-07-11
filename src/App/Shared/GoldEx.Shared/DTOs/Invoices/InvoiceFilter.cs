using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record InvoiceFilter(InvoicePaymentStatus? Status, DateTime? Start, DateTime? End);