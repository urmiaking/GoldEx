using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Server.Application.Abstractions.Sms;

public enum Status
{
    [Display(Name = "موفق")]
    Success,

    [Display(Name = "حساب کاربری نامعتبر")]
    InvalidAccount,

    [Display(Name = "اعتبار ناکافی")]
    InsufficientBalance,

    [Display(Name = "محدودیت استفاده")]
    QuotationLimit,

    [Display(Name = "پیغام نامعتبر")]
    InvalidMessage,

    [Display(Name = "مواجه با خطا")]
    Failed
}