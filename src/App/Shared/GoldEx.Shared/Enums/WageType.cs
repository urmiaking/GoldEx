using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum WageType
{
    [Display(Name = "درصد")]
    Percent = 0,

    [Display(Name = "مبلغ ثابت")]
    Fixed = 1
}
