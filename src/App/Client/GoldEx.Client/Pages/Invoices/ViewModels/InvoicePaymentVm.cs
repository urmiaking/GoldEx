using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoicePaymentVm
{
    [Display(Name = "تاریخ پرداخت")]
    public DateTime? PaymentDate { get; set; }

    [Display(Name = "کد پیگیری")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "یادداشت")]
    public string? Note { get; set; }

    [Display(Name = "مبلغ")]
    public decimal Amount { get; set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "روش پرداخت")]
    public GetPaymentMethodResponse? PaymentMethod { get; set; }
}