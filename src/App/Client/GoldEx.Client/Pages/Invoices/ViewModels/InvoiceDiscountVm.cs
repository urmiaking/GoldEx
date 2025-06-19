using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using ValidationException = FluentValidation.ValidationException;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceDiscountVm
{
    [Display(Name = "مبلغ")]
    public decimal Amount { get;  set; }

    [Display(Name = "بابت")]
    public string? Description { get; set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public string? ExchangeRateLabel { get; set; }
    public string AmountAdornmentText { get; set; } = default!;
    public bool AmountMenuOpen { get; set; }

    public static InvoiceDiscountDto ToRequest(InvoiceDiscountVm item)
    {
        if (item.PriceUnit is null)
            throw new ValidationException("واحد ارزی تخفیف وارد نشده است");

        return new InvoiceDiscountDto(item.Amount, item.Description, item.PriceUnit.Id);
    }
}