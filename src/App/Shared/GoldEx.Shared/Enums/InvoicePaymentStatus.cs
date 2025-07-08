using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum InvoicePaymentStatus
{
    [Display(Name = "تسویه حساب")]
    Paid = 0,

    [Display(Name = "دارای بدهی")]
    HasDebt = 1,

    [Display(Name = "موعد سررسید گذشته")]
    Overdue = 2
}