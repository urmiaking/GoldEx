using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentSide
{
    [Display(Name = "دریافت از مشتری")]
    Receive = 0,

    [Display(Name = "پرداخت به مشتری")]
    Pay = 1
}
public static class PaymentSideExtensions
{
    public static string GetDisplayTitle(this PaymentSide paymentSide)
    {
        return paymentSide switch
        {
            PaymentSide.Receive => "دریافت",
            PaymentSide.Pay => "پرداخت",
            _ => throw new ArgumentOutOfRangeException(nameof(paymentSide), paymentSide, null)
        };
    }
}