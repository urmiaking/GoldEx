using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PaymentType
{
    [Display(Name = "حساب های داخلی")]
    InternalCash = 0,

    [Display(Name = "طلای شکسته")]
    UsedGoldInventory = 1,

    [Display(Name = "طلای آبشده")]
    MoltenGoldInventory = 2,

    [Display(Name = "حواله")]
    CustomerTransfer = 3,

    [Display(Name = "حواله شده")]
    TransferedPayment = 4,

    [Display(Name = "چک")]
    Check = 5
}

public static class PaymentTypeExtensions
{
    public static string GetDisplayTitle(this PaymentType paymentType)
    {
        return paymentType switch
        {
            PaymentType.InternalCash => "پرداخت نقدی",
            PaymentType.UsedGoldInventory => "پرداخت با طلای شکسته",
            PaymentType.MoltenGoldInventory => "پرداخت با طلای آبشده",
            PaymentType.CustomerTransfer => "حواله مشتری",
            PaymentType.TransferedPayment => "حواله شده",
            PaymentType.Check => "پرداخت با چک",
            _ => throw new ArgumentOutOfRangeException(nameof(paymentType), paymentType, null)
        };
    }
}