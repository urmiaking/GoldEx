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

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public string? ExchangeRateLabel { get; set; }
    public string AmountAdornmentText { get; set; } = default!;
    public bool AmountMenuOpen { get; set; }
}