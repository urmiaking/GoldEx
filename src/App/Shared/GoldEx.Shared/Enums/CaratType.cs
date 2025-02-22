using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CaratType
{
    [Display(Name = "17 عیار")]
    SevenTeen = 1,

    [Display(Name = "18 عیار")]
    Eighteen = 2,

    [Display(Name = "21 عیار")]
    TwentyOne = 3,

    [Display(Name = "22 عیار")]
    TwentyTwo = 4,

    [Display(Name = "24 عیار")]
    TwentyFour = 5
}