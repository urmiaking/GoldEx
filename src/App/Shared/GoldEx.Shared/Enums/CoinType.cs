using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum CoinType
{
    [Display(Name = "سکه")]
    Coin = 0,

    [Display(Name = "پارسیان")]
    ParsianCoin = 1
}