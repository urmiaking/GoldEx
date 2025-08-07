using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.PaymentVouchers.ViewModels;

public class PaymentVoucherVm
{
    public Guid? Id { get; set; }

    [Display(Name = "شماره سند")]
    [Required(ErrorMessage = "شماره سند الزامی است.")]
    public long VoucherNumber { get; set; }

    [Display(Name = "شرح")]
    [Required(ErrorMessage = "شرح سند الزامی است.")]
    [MaxLength(500, ErrorMessage = "شرح نمی تواند بیشتر از 150 کاراکتر باشد.")]
    public string? Description { get; set; }

    [Display(Name = "تاریخ پرداخت")]
    [Required(ErrorMessage = "تاریخ پرداخت الزامی است.")]
    public DateTime? PaymentDate { get; set; } = DateTime.Now;

    [Display(Name = "مبلغ")]
    [Required(ErrorMessage = "مبلغ الزامی است.")]
    public decimal? Amount { get; set; }

    [Display(Name = "واحد مبلغ")]
    [Required(ErrorMessage = "واحد مبلغ الزامی است.")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "حساب پرداختی")]
    [Required(ErrorMessage = "حساب پرداختی الزامی است.")]
    public GetFinancialAccountTitleResponse? SourceFinancialAccount { get; set; }

    [Display(Name = "حساب دریافتی")]
    [Required(ErrorMessage = "حساب دریافتی الزامی است.")]
    public GetFinancialAccountTitleResponse? DestinationFinancialAccount { get; set; }

    [Display(Name = "نرخ تبدیل")] 
    public decimal? ExchangeRate { get; set; }

    public CustomerVm? Customer { get; set; }

    [Display(Name = "نوع سند")]
    public PaymentVoucherType VoucherType { get; set; } = PaymentVoucherType.PrepaymentToSupplier;

    public static PaymentVoucherVm CreateFrom(GetPaymentVoucherResponse response)
    {
        return new PaymentVoucherVm
        {
            Id = response.Id,
            VoucherNumber = response.VoucherNumber,
            Description = response.Description,
            Amount = response.Amount,
            SourceFinancialAccount = response.SourceFinancialAccount,
            DestinationFinancialAccount = response.DestinationFinancialAccount,
            ExchangeRate = response.ExchangeRate,
            Customer = CustomerVm.CreateFrom(response.Customer),
            PaymentDate = response.PaymentDate,
            PriceUnit = response.PriceUnit,
            VoucherType = response.VoucherType
        };
    }

    public static PaymentVoucherRequestDto ToRequest(PaymentVoucherVm? model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (model.PriceUnit == null)
            throw new ArgumentNullException(nameof(model.PriceUnit));

        if (model.SourceFinancialAccount == null)
            throw new ArgumentNullException(nameof(model.SourceFinancialAccount));

        if (model.DestinationFinancialAccount == null)
            throw new ArgumentNullException(nameof(model.DestinationFinancialAccount));

        if (string.IsNullOrEmpty(model.Description))
            throw new ArgumentNullException(nameof(model.Description));

        if (!model.PaymentDate.HasValue)
            throw new ArgumentNullException(nameof(model.PaymentDate));

        if (!model.Amount.HasValue)
            throw new ArgumentNullException(nameof(model.Amount));

        return new PaymentVoucherRequestDto(model.Id,
            model.VoucherNumber,
            model.Amount.Value,
            model.ExchangeRate,
            model.Description,
            DateOnly.FromDateTime(model.PaymentDate.Value), 
            model.VoucherType,
            model.PriceUnit.Id,
            model.SourceFinancialAccount.Id,
            model.DestinationFinancialAccount.Id);
    }
}