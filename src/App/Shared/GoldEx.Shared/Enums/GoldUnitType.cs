using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum GoldUnitType
{
    [Display(Name = "گرم")]
    Gram = 0,

    [Display(Name = "مثقال")]
    Mesghal = 1
}