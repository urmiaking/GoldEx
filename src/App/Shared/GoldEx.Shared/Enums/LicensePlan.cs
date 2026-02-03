using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum LicensePlan
{
    [Display(Name = "نسخه فعالسازی نشده")]
    Unregistered = 0,

    [Display(Name = "نسخه پایه")]
    Trial = 1,

    [Display(Name = "نسخه کامل")]
    Regular = 2
}