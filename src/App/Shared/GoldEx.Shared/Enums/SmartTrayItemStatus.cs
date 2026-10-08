using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum SmartTrayItemStatus
{
    [Display(Name = "روی پیش‌خوان")]
    InTray = 1,

    [Display(Name = "فروخته شده")]
    Sold = 2,

    [Display(Name = "بازگشت به ویترین")]
    Returned = 3,

    [Display(Name = "مفقود شده")]
    Missing = 4
}
