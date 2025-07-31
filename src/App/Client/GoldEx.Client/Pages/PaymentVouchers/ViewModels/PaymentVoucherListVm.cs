using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.PaymentVouchers.ViewModels;

public class PaymentVoucherListVm
{
    public Guid Id { get; set; }

    [Display(Name = "شماره سند")]
    public long VoucherNumber { get; set; }

    [Display(Name = "شرح")]
    public string? Description { get; set; }

    [Display(Name = "تاریخ ثبت")]
    public DateOnly PaymentDate { get; set; }

    [Display(Name = "مبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "واحد مبلغ")]
    public string? PriceUnit { get; set; }

    [Display(Name = "وضعیت")]
    public VoucherStatus VoucherStatus { get; set; }

    [Display(Name = "تامین کننده")]
    public string? SupplierName { get; set; }

    [Display(Name = "شماره تماس")]
    public string? SupplierPhoneNumber { get; set; }

    [Display(Name = "پرداخت از حساب")] 
    public FinancialAccountType FinancialAccountType { get; set; }

    public static PaymentVoucherListVm CreateFrom(GetPaymentVoucherListResponse response)
    {
        return new PaymentVoucherListVm
        {
            Id = response.Id,
            Amount = response.Amount,
            PriceUnit = response.PriceUnit,
            Description = response.Description,
            PaymentDate = response.PaymentDate,
            VoucherNumber = response.VoucherNumber,
            VoucherStatus = response.VoucherStatus,
            SupplierName = response.SupplierName,
            SupplierPhoneNumber = response.SupplierPhoneNumber,
            FinancialAccountType = response.FinancialAccountType
        };
    }
}