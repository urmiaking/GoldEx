using GoldEx.Sdk.Common.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GoldEx.Sdk.Common.Definitions;

public enum MarketType
{
    [Display(Name = "طلا")]
    [DisplayOrder(1)]
    Gold = 0,

    [Display(Name = "ارز")]
    [DisplayOrder(3)]
    Currency = 1,

    [Display(Name = "سکه")]
    [DisplayOrder(4)]
    Coin = 2,

    [Display(Name = "حباب سکه")]
    [DisplayOrder(5)]
    BubbleCoin = 3,

    [Display(Name = "پارسیان")]
    [DisplayOrder(6)]
    ParsianCoin = 4,

    [Display(Name = "فلزات گرانبها")]
    [DisplayOrder(2)]
    Ounce = 5,

    [Display(Name = "نقره")]
    [DisplayOrder(7)]
    Silver = 6
}

public static class EnumExtensions
{
    public static int? GetDisplayOrder(this MarketType value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DisplayOrderAttribute>();
        return attr?.Order;
    }
}
