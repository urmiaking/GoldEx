using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum UnitType
{
    [Display(Name = "ریال")]
    IRR,

    [Display(Name = "دلار")]
    USD,

    [Display(Name = "یورو")]
    EUR,

    [Display(Name = "پوند")]
    GBP,

    [Display(Name = "لیر")]
    TRY,

    [Display(Name = "دینار")]
    AED,

    [Display(Name = "گرم")]
    Gram
}