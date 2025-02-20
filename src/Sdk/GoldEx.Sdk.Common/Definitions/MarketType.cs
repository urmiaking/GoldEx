using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Common.Definitions;

public enum MarketType
{
    [Display(Name = "سکه")]
    Coin,

    [Display(Name = "طلا")]
    Gold,

    [Display(Name = "ارز")]
    Currency,

    [Display(Name = "حباب سکه")]
    BubbleCoin,

    [Display(Name = "پارسیان")]
    ParsianCoin
}