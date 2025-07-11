using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ProductStatus
{
    [Display(Name = "موجود")]
    Available = 0,

    [Display(Name = "فروخته شده")]
    Sold = 1
}