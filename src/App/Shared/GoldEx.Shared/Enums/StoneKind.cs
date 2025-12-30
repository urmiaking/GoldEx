using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum StoneKind
{
    [Display(Name = "قیمتی")]
    Precious = 1,     

    [Display(Name = "نیمه‌ قیمتی")]
    SemiPrecious = 2,  

    [Display(Name = "تزئینی")]
    Decorative = 3   
}