using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record InvoiceFilter(InvoicePaymentStatus? Status, InvoiceType? InvoiceType, TradeScale? TradeScale, DateTime? Start, DateTime? End);