using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum TradeScale
{
    [Display(Name = "خرده")]
    Retail = 1,

    [Display(Name = "عمده")]
    Wholesale = 0
}