using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceListVm
{
    public Guid Id { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public long InvoiceNumber { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string AmountUnit { get; set; } = default!;
    public InvoicePaymentStatus PaymentStatus { get; set; }

    public static InvoiceListVm CreateFrom(GetInvoiceResponse response)
    {
        return new InvoiceListVm
        {
            Id = response.Id,
            CustomerFullName = response.CustomerFullName,
            InvoiceNumber = response.InvoiceNumber,
            InvoiceDate = response.InvoiceDate,
            DueDate = response.DueDate,
            TotalAmount = response.TotalAmount,
            AmountUnit = response.AmountUnit,
            PaymentStatus = response.PaymentStatus
        };
    }
}