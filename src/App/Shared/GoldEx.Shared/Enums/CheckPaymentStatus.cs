using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CheckPaymentStatus
{
    [Display(Name = "در انتظار وصول")]
    Pending = 1,

    [Display(Name = "وصول شده")]
    Accepted = 2,

    [Display(Name = "برگشت خورده")]
    Returned = 3
}