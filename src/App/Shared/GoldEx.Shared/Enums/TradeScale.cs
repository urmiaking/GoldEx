using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum TradeScale
{
    [Display(Name = "عمده")]
    Wholesale = 0,

    [Display(Name = "خرده")]
    Retail = 1
}