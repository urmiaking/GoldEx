using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CaratType
{
    [Display(Name = "18 عیار (750)")]
    Eighteen = 0,

    [Display(Name = "21 عیار (875)")]
    TwentyOne = 1,

    [Display(Name = "22 عیار (916)")]
    TwentyTwo = 2,

    [Display(Name = "24 عیار (999.9)")]
    TwentyFour = 3
}