using System.ComponentModel.DataAnnotations;

namespace GoldEx.Sdk.Common.Definitions;

public enum PriceType
{
    [Display(Name = "سکه")]
    Coin,

    [Display(Name = "طلا")]
    Gold,

    [Display(Name = "ارز")]
    Currency
}