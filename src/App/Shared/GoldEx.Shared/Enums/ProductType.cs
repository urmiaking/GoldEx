using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ProductType
{
    [Display(Name = "جواهر")]
    Jewelry = 1,

    [Display(Name = "طلا")]
    Gold = 2,

    [Display(Name = "طلای آب شده")]
    MoltenGold = 3
}