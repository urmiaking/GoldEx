using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Common.Definitions;

public enum MarketType
{
    [Display(Name = "طلا")]
    Gold = 0,

    [Display(Name = "ارز")]
    Currency = 1,

    [Display(Name = "سکه")]
    Coin = 2,

    [Display(Name = "حباب سکه")]
    BubbleCoin = 3,

    [Display(Name = "پارسیان")]
    ParsianCoin = 4
}