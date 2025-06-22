
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Invoices;
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
    [Required(ErrorMessage = "وارد کردن روش پرداخت الزامی است")]
    public GetPaymentMethodResponse? PaymentMethod { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public string? ExchangeRateLabel { get; set; }
    public string AmountAdornmentText { get; set; } = default!;
    public bool AmountMenuOpen { get; set; }

    public static InvoicePaymentDto ToRequest(InvoicePaymentVm item)
    {
        if (item.PriceUnit is null)
            throw new FluentValidation.ValidationException("واحد ارزی برای پرداختی ها مشخص نشده است");

        if (item.PaymentMethod is null)
            throw new FluentValidation.ValidationException("روش پرداخت برای پرداختی ها مشخص نشده است");

        if (!item.PaymentDate.HasValue)
            throw new FluentValidation.ValidationException("تاریخ پرداخت مشخص نشده است");

        return new InvoicePaymentDto(item.Amount, item.ExchangeRate, item.PaymentDate.Value, item.ReferenceNumber, item.Note,
            item.PaymentMethod.Id, item.PriceUnit.Id);
    }

    public static InvoicePaymentVm CreateFrom(GetInvoicePaymentResponse response)
    {
        return new InvoicePaymentVm
        {
            Amount = response.Amount,
            PriceUnit = response.PriceUnit,
            PaymentMethod = response.PaymentMethod,
            PaymentDate = response.PaymentDate,
            ReferenceNumber = response.ReferenceNumber,
            Note = response.Note,
            ExchangeRate = response.ExchangeRate,
            AmountAdornmentText = response.PriceUnit.Title
        };
    }
}