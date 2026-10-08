using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum SmartTrayStatus
{
    [Display(Name = "فعال روی پیش‌خوان")]
    Active = 1,

    [Display(Name = "در حال تطبیق با ویترین")]
    Auditing = 2,

    [Display(Name = "تکمیل و بسته شده")]
    Completed = 3,

    [Display(Name = "دارای کسری و مغایرت")]
    HasDiscrepancy = 4,

    [Display(Name = "لغو شده")]
    Cancelled = 5
}
