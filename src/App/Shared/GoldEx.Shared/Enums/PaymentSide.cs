using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentSide
{
    [Display(Name = "دریافت از مشتری")]
    Receive = 0,

    [Display(Name = "پرداخت به مشتری")]
    Pay = 1
}