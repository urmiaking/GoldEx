using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum PriceProviderType
{
    [Display(Name = "غیرفعال/دستی")]
    Manual = 0,

    [Display(Name = "سیگنال (Signal.ir)")]
    Signal = 1,

    [Display(Name = "سایت طلا (Tala.ir)")]
    TalaIr = 2,

    [Display(Name = "وب سرویس بورس (BrsApi)")]
    BrsApi = 3,

    [Display(Name = "اتحادیه طلا (Tjgu)")]
    Tjgu= 4
}