using GoldEx.Shared.DTOs.PriceUnits;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Invoices;

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

    public static InvoiceExtraCostsDto ToRequest(InvoiceExtraCostVm item)
    {
        if (item.PriceUnit is null)
            throw new FluentValidation.ValidationException("واحد ارزی مخارج جانبی وارد نشده است");

        return new InvoiceExtraCostsDto(item.Amount, item.ExchangeRate, item.Description, item.PriceUnit.Id);
    }

    public static InvoiceExtraCostVm CreateFrom(GetInvoiceExtraCostsResponse response)
    {
        return new InvoiceExtraCostVm
        {
            Amount = response.Amount,
            Description = response.Description,
            PriceUnit = response.PriceUnit,
            AmountAdornmentText = response.PriceUnit.Title,
            ExchangeRate = response.ExchangeRate
        };
    }
}