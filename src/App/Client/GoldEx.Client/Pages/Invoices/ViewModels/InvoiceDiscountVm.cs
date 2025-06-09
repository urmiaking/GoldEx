using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceDiscountVm
{
    [Display(Name = "مبلغ")]
    public decimal Amount { get;  set; }

    [Display(Name = "بابت")]
    public string? Description { get; set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
}