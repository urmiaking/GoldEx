using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceListVm
{
    public Guid Id { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public long InvoiceNumber { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateOnly? DueDate { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public TradeScale TradeScale { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalUnpaidAmount { get; set; }
    public string PriceUnit { get; set; } = default!;

    public decimal? TotalUnpaidAmountSecondary { get; set; }
    public string? SecondaryPriceUnit { get; set; }

    public bool ShowDetails { get; set; }

    public InvoicePaymentStatus PaymentStatus { get; set; }

    public static InvoiceListVm CreateFrom(GetInvoiceListResponse listResponse)
    {
        return new InvoiceListVm
        {
            Id = listResponse.Id,
            CustomerFullName = listResponse.CustomerFullName,
            InvoiceNumber = listResponse.InvoiceNumber,
            InvoiceDate = listResponse.InvoiceDate,
            InvoiceType = listResponse.InvoiceType,
            TradeScale = listResponse.TradeScale,
            DueDate = listResponse.DueDate,
            CreatedAt = listResponse.CreatedAt,
            TotalAmount = listResponse.TotalAmount,
            PriceUnit = listResponse.PriceUnit,
            TotalUnpaidAmount = listResponse.TotalUnpaidAmount,
            PaymentStatus = listResponse.PaymentStatus,
            TotalUnpaidAmountSecondary = listResponse.TotalUnpaidAmountSecondary,
            SecondaryPriceUnit = listResponse.SecondaryPriceUnit
        };
    }
}