using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoicePaymentVm
{
    public DateTime PaymentDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Note { get; private set; }

    public decimal Amount { get; private set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; private set; }
    public GetPaymentMethodResponse? PaymentMethod { get; private set; }
}