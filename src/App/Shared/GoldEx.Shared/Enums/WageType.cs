using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum WageType
{
    [Display(Name = "درصد")]
    Percent = 1,

    [Display(Name = "ریال")]
    Rial = 2,

    [Display(Name = "دلار")]
    Dollar = 3
}
