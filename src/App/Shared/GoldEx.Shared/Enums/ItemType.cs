using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ItemType
{
    [Display(Name = "مصنوعات طلا")]
    Product = 1,

    [Display(Name = "سکه")]
    Coin = 2,

    [Display(Name = "ارز")]
    Currency = 3
}