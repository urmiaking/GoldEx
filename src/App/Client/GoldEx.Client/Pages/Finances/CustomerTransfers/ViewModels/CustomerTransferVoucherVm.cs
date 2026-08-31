using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;

public class CustomerTransferVoucherVm
{
    public Guid? Id { get; set; }

    [Display(Name = "شماره حواله")]
    public long VoucherNumber { get; set; }

    [Display(Name = "تاریخ حواله")]
    public DateTime? TransferDate { get; set; } = DateTime.Now;

    [Display(Name = "مشتری حواله‌دهنده (فرستنده)")]
    public CustomerVm? SourceCustomer { get; set; }

    [Display(Name = "مشتری دریافت‌کننده (گیرنده)")]
    public CustomerVm? DestinationCustomer { get; set; }

    [Display(Name = "واحد قیمت / نوع دارایی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "مبلغ / وزن حواله")]
    public decimal Amount { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public Guid? SourceInvoiceId { get; set; }
    public Guid? DestinationInvoiceId { get; set; }

    [Display(Name = "فاکتور مبدا (خرید/فروش فرستنده)")]
    public GetTinyInvoiceResponse? SourceInvoice { get; set; }

    [Display(Name = "فاکتور مقصد (خرید/فروش گیرنده)")]
    public GetTinyInvoiceResponse? DestinationInvoice { get; set; }

    [Display(Name = "شرح / توضیحات")]
    public string? Description { get; set; }
}
