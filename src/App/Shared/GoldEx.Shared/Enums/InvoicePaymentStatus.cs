using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum InvoicePaymentStatus
{
    [Display(Name = "تسویه حساب")]
    Paid = 0,

    [Display(Name = "دارای بدهی")]
    PartiallyPaid = 1,

    [Display(Name = "پرداخت نشده")]
    Unpaid = 2
}