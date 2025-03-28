using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CustomerType
{
    [Display(Name = "حقیقی")]
    Individual,

    [Display(Name = "حقوقی")]
    Legal
}