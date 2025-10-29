using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum WarehouseActionType
{
    [Display(Name = "ورود")]
    In = 0,

    [Display(Name = "خروج")]
    Out = 1
}