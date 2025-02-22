using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum WageType
{
    [Display(Name = "درصدی")]
    Percent = 1,

    [Display(Name = "ریالی")]
    Rial = 2,

    [Display(Name = "دلاری")]
    Dollar = 3
}
