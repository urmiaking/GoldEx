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

    public static InvoiceListVm CreateFrom(GetInvoiceListResponse listResponse)
    {
        return new InvoiceListVm
        {
            Id = listResponse.Id,
            CustomerFullName = listResponse.CustomerFullName,
            InvoiceNumber = listResponse.InvoiceNumber,
            InvoiceDate = listResponse.InvoiceDate,
            DueDate = listResponse.DueDate,
            TotalAmount = listResponse.TotalAmount,
            AmountUnit = listResponse.AmountUnit,
            PaymentStatus = listResponse.PaymentStatus
        };
    }
}