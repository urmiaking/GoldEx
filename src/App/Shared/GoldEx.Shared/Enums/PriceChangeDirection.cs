using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PriceChangeDirection
{
    [Display(Name = "بدون تغییر")]
    None = 0,

    [Display(Name = "افزایشی")]
    Up = 1,

    [Display(Name = "کاهشی")]
    Down = 2
}
