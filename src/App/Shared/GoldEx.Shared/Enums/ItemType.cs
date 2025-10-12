using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ItemType
{
    [Display(Name = "طلا و جواهر")]
    Product = 1,

    [Display(Name = "طلای آبشده")]
    MoltenGold = 2,

    [Display(Name = "سکه")]
    Coin = 3,

    [Display(Name = "ارز")]
    Currency = 4,

    [Display(Name = "طلای دست دوم")]
    UsedProduct = 5
}