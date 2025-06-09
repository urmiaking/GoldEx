using GoldEx.Shared.DTOs.PriceUnits;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceExtraCostVm
{
    [Display(Name = "مبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "بابت")]
    public string? Description { get; set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
}