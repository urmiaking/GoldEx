using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ItemStatus
{
    [Display(Name = "موجود")]
    Available = 0,

    [Display(Name = "فروخته شده")]
    Sold = 1,

    [Display(Name = "ذوب شده")]
    Melted = 2
}