using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Dashboard;

public class TopUnpaidInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceType InvoiceType { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public decimal TotalUnpaidAmount { get; set; }
    public string PriceUnit { get; set; } = "تومان";
    public DateOnly InvoiceDate { get; set; }
}
